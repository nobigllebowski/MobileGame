using System;
using Nation.Core.Countries;
using Nation.Core.Map;
using Nation.Core.Map.Layers;
using Nation.Core.Models;
using Nation.Core.Nation;
using Nation.Core.Signals;
using Nation.Game.Bootstrap;
using Nation.Game.Map;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>
    /// Main gameplay space: HUD, time controls and the real world map. The map itself is rendered in the scene;
    /// this tab hosts the transparent gesture surface, the label and capital overlays, the layer switcher, the home
    /// button and the country sheet, all driven by map signals rather than polling.
    /// </summary>
    public sealed class WorldTab : GameTab
    {
        private readonly MapWorldBuilder.Result _map;

        private Label _date;
        private Label _treasury;
        private Label _gdp;
        private Label _population;
        private Label _speedLabel;
        private Label _legend;
        private TimeControls _time;
        private VisualElement _viewport;
        private VisualElement _labelLayer;
        private VisualElement _capitalLayer;
        private MapGestureElement _gestures;
        private FilterChip _politicalChip;
        private FilterChip _economyChip;
        private FilterChip _moreChip;

        private CountrySelectionController _selection;
        private MapLabelController _labels;
        private MapCapitalRenderer _capitals;
        private MapLayerController _layerController;
        private IDisposable _selectionSubscription;
        private IDisposable _layerSubscription;
        private BottomSheet _countrySheet;
        private string _sheetCountryId;
        private bool _suppressDeselect;
        private Rect _viewportRect;

        public WorldTab(GameContext context, GameShellScreen shell, MapWorldBuilder.Result map) : base(context, shell, "tab-world")
        {
            _map = map;
        }

        protected override void Build()
        {
            var definition = Session.PlayerDefinition;

            var hud = new VisualElement();
            hud.AddToClassList("hud");
            var identity = new VisualElement();
            identity.AddToClassList("hud__identity");
            identity.Add(new FlagElement(definition.Flag, "flag--hud"));
            var text = new VisualElement();
            text.AddToClassList("hud__text");
            text.Add(Typography.Heading(Loc.Get(definition.NameKey), "hud__name"));
            _date = Typography.Caption(string.Empty, "hud__date");
            text.Add(_date);
            identity.Add(text);
            hud.Add(identity);

            var stats = new VisualElement();
            stats.AddToClassList("hud__stats");
            _treasury = HudChip(stats, Loc.Get("stat.treasury"), "hud__chip--gold");
            _gdp = HudChip(stats, Loc.Get("stat.gdp"), null);
            _population = HudChip(stats, Loc.Get("stat.population"), null);
            hud.Add(stats);
            Add(hud);

            _viewport = new VisualElement { name = "map-viewport" };
            _viewport.AddToClassList("map-viewport");
            _viewport.pickingMode = PickingMode.Ignore;
            Add(_viewport);

            _gestures = new MapGestureElement();
            _viewport.Add(_gestures);

            _labelLayer = Overlay("map-labels");
            _capitalLayer = Overlay("map-capitals");
            _viewport.Add(new MapVignetteElement());

            var layerBar = new VisualElement();
            layerBar.AddToClassList("layer-bar");
            _politicalChip = LayerChip(layerBar, MapLayerKind.Political);
            _economyChip = LayerChip(layerBar, MapLayerKind.Economy);
            _moreChip = new FilterChip(Loc.Get("world.layers"), true);
            _moreChip.AddToClassList("layer-chip");
            _moreChip.Clicked += OpenLayerSheet;
            layerBar.Add(_moreChip);
            _viewport.Add(layerBar);

            _legend = Typography.Caption(string.Empty, "layer-legend");
            _legend.pickingMode = PickingMode.Ignore;
            _viewport.Add(_legend);

            var home = Buttons.Icon(IconKind.Pin, ReturnHome, "map-home");
            Buttons.GlyphOf(home).Color = Palette.Text;
            _viewport.Add(home);

            var timeBar = new VisualElement();
            timeBar.AddToClassList("time-bar");
            _speedLabel = Typography.Caption(string.Empty, "time-bar__label");
            timeBar.Add(_speedLabel);
            _time = new TimeControls();
            _time.SpeedRequested += speed => Session.SetSpeed(speed);
            timeBar.Add(_time);
            Add(timeBar);

            if (_map != null)
            {
                WireMap();
            }
            else
            {
                _viewport.Add(new EmptyState(IconKind.Globe, Loc.Get("select.empty.title"), Loc.Get("world.selection_hint")));
            }

            Refresh();
            OnSpeedChanged(Session.Speed);
            UpdateLayerChips();
        }

        private void WireMap()
        {
            var service = Context.Map;
            _selection = new CountrySelectionController(service, _map.Camera, _gestures);
            _labels = new MapLabelController(service, _map.Camera, _labelLayer, Loc);
            _capitals = new MapCapitalRenderer(service, _map.Camera, _capitalLayer, Loc);
            _labels.SetPlayer(Session.PlayerCountryId);
            _capitals.SetPlayer(Session.PlayerCountryId);
            _layerController = new MapLayerController(service, _map.Renderer, Context.Signals, () => Session.PlayerCountryId);

            _selectionSubscription = Context.Signals.Subscribe<CountrySelectedSignal>(OnCountrySelected);
            _layerSubscription = Context.Signals.Subscribe<MapLayerChangedSignal>(_ => UpdateLayerChips());

            _viewport.RegisterCallback<GeometryChangedEvent>(_ => UpdateViewportRect());
            _viewport.schedule.Execute(() =>
            {
                UpdateViewportRect();
                _selection.ReturnHome(Session.PlayerCountryId);
                UI.Toast(Loc.Get("world.selection_hint"));
            }).StartingIn(60);
        }

        private void UpdateViewportRect()
        {
            if (_map == null || _viewport.panel == null)
            {
                return;
            }

            var bound = _viewport.worldBound;
            if (bound.width <= 0 || bound.height <= 0)
            {
                return;
            }

            // PanelToScreen flips the vertical axis, so the panel's top edge becomes the screen rect's top.
            var topLeft = PanelCoordinates.PanelToScreen(_viewport.panel, new Vector2(bound.xMin, bound.yMin));
            var bottomRight = PanelCoordinates.PanelToScreen(_viewport.panel, new Vector2(bound.xMax, bound.yMax));
            _viewportRect = new Rect(topLeft.x, bottomRight.y, bottomRight.x - topLeft.x, topLeft.y - bottomRight.y);
            ApplyViewport();
        }

        private void ApplyViewport()
        {
            var rect = _viewportRect;
            if (_countrySheet != null && _countrySheet.panel != null)
            {
                var bottom = PanelCoordinates.PanelToScreen(_countrySheet.panel, new Vector2(0f, _countrySheet.worldBound.yMin)).y;
                if (bottom > rect.y)
                {
                    rect = new Rect(rect.x, bottom, rect.width, Mathf.Max(80f, rect.yMax - bottom));
                }
            }

            _map.Camera.SetViewport(rect);
        }

        public override void OnShow()
        {
            if (!IsBuilt)
            {
                return;
            }

            Refresh();
            OnSpeedChanged(Session.Speed);
            _labels?.Refresh();
            _capitals?.Refresh();
        }

        public override void OnTick()
        {
            Refresh();
        }

        public override void OnSpeedChanged(GameSpeed speed)
        {
            _time.SetSpeed(speed);
            _speedLabel.text = Loc.Get("speed." + speed.ToString().ToLowerInvariant());
        }

        public override void OnDestroyed()
        {
            _selectionSubscription?.Dispose();
            _layerSubscription?.Dispose();
            _selection?.Dispose();
            _labels?.Dispose();
            _capitals?.Dispose();
            _layerController?.Dispose();
        }

        private void Refresh()
        {
            var country = Session.PlayerCountry;
            _date.text = Format.Date(Session.World.CurrentDate);
            _treasury.text = Format.Money(country.Treasury);
            _gdp.text = Format.Money(country.Gdp);
            _population.text = Format.Population(country.Population);
        }

        private void ReturnHome()
        {
            _selection?.ReturnHome(Session.PlayerCountryId);
        }

        // ---------- layers ----------

        private FilterChip LayerChip(VisualElement parent, MapLayerKind kind)
        {
            var chip = new FilterChip(Loc.Get(MapLayerKeys.Name(kind)), false);
            chip.AddToClassList("layer-chip");
            chip.Clicked += () => Context.Map.SetLayer(kind);
            parent.Add(chip);
            return chip;
        }

        private void UpdateLayerChips()
        {
            var active = Context.Map.ActiveLayer;
            _politicalChip.SetSelected(active == MapLayerKind.Political);
            _economyChip.SetSelected(active == MapLayerKind.Economy);
            var other = active != MapLayerKind.Political && active != MapLayerKind.Economy;
            _moreChip.SetValue(other ? Loc.Get(MapLayerKeys.Name(active)) : null);

            var provider = Context.Map.ActiveLayerProvider;
            if (provider.Style == MapLayerStyle.Gradient && provider.Availability != MapLayerAvailability.Unavailable)
            {
                _legend.text = Loc.Get(provider.LegendLowKey) + "  →  " + Loc.Get(provider.LegendHighKey);
            }
            else if (provider.Availability == MapLayerAvailability.Unavailable)
            {
                _legend.text = Loc.Get("layer.availability.unavailable");
            }
            else
            {
                _legend.text = Loc.Get(provider.DescriptionKey);
            }
        }

        private void OpenLayerSheet()
        {
            var sheet = new BottomSheet { AllowExpanded = false };
            sheet.SetSnapPoints(0.36f, 0.66f, 0.9f);
            sheet.Header.Add(Typography.SectionTitle(Loc.Get("world.layers_title"), "sheet__title"));

            foreach (var layer in Context.Map.Layers.Layers)
            {
                var captured = layer;
                var row = new VisualElement();
                row.AddToClassList("layer-row");
                row.EnableInClassList("layer-row--active", layer.Kind == Context.Map.ActiveLayer);
                row.EnableInClassList("layer-row--unavailable", layer.Availability == MapLayerAvailability.Unavailable);

                var texts = new VisualElement();
                texts.AddToClassList("layer-row__text");
                texts.Add(Typography.Body(Loc.Get(layer.NameKey), "layer-row__name"));
                texts.Add(Typography.Caption(Loc.Get(layer.DescriptionKey), "layer-row__description"));
                row.Add(texts);
                row.Add(new Badge(Loc.Get("layer.availability." + layer.Availability.ToString().ToLowerInvariant()), LevelFor(layer.Availability)));
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    if (captured.Availability == MapLayerAvailability.Unavailable)
                    {
                        UI.ToastKey("toast.feature_unavailable", ToastKind.Warning);
                        return;
                    }

                    Context.Map.SetLayer(captured.Kind);
                    UI.Sheets.Dismiss();
                });
                sheet.Body.Add(row);
            }

            UI.Sheets.Show(sheet, SheetState.Half);
        }

        private static StatusLevel LevelFor(MapLayerAvailability availability)
        {
            switch (availability)
            {
                case MapLayerAvailability.Available: return StatusLevel.Positive;
                case MapLayerAvailability.Partial: return StatusLevel.Neutral;
                default: return StatusLevel.Warning;
            }
        }

        // ---------- selection and country sheet ----------

        private void OnCountrySelected(CountrySelectedSignal signal)
        {
            _map.Renderer.SetSelected(signal.CountryId);
            _capitals?.SetSelected(signal.CountryId);

            if (!signal.IsSelection)
            {
                if (_countrySheet != null)
                {
                    _suppressDeselect = true;
                    UI.Sheets.Dismiss();
                    _suppressDeselect = false;
                    _countrySheet = null;
                    _sheetCountryId = null;
                    ApplyViewport();
                }

                return;
            }

            if (Context.Map.Catalog.TryGet(signal.CountryId, out var country))
            {
                ShowCountrySheet(country);
            }
        }

        private void ShowCountrySheet(MapCountry country)
        {
            var isPlayer = country.Id == Session.PlayerCountryId;
            var simulated = Session.World.TryGetCountry(country.Id, out var state);
            Context.Countries.TryGet(country.Id, out var definition);

            var sheet = new BottomSheet();
            sheet.SetSnapPoints(0.36f, 0.62f, 0.9f);
            _sheetCountryId = country.Id;

            var header = new VisualElement();
            header.AddToClassList("sheet__country");
            header.Add(new FlagElement(definition != null ? definition.Flag : FlagSpec.Empty, "flag--sheet"));
            var titles = new VisualElement();
            titles.AddToClassList("sheet__country-text");
            titles.Add(Typography.Heading(Loc.Get(country.NameKey), "sheet__country-name"));
            var capital = country.HasCapital ? Loc.Get(country.CapitalKey) : Loc.Get(TypeKey(country.Type));
            titles.Add(Typography.Caption(capital + "  ·  " + country.Continent, "sheet__country-meta"));
            header.Add(titles);

            if (isPlayer)
            {
                header.Add(new Badge(Loc.Get("relationship.own"), StatusLevel.Positive));
            }
            else if (simulated)
            {
                header.Add(new Badge(Loc.Get("relationship.neutral"), StatusLevel.Neutral));
            }
            else
            {
                header.Add(new Badge(Loc.Get("layer.availability.unavailable"), StatusLevel.Warning));
            }

            sheet.Header.Add(header);

            var stats = new VisualElement();
            stats.AddToClassList("sheet__stats");
            if (simulated)
            {
                stats.Add(Mini(Loc.Get("stat.gdp"), Format.Money(state.Gdp)));
                stats.Add(Mini(Loc.Get("stat.population"), Format.Population(state.Population)));
                stats.Add(Mini(Loc.Get("stat.stability"), Format.Percent(state.Stability)));
                stats.Add(Mini(Loc.Get("world.relationship"), isPlayer ? Loc.Get("relationship.own") : Loc.Get("relationship.neutral")));
            }
            else
            {
                stats.Add(Mini(Loc.Get("stat.gdp") + " · " + Loc.Get("world.estimate"), country.GdpEstimate > 0 ? Format.Money(country.GdpEstimate) : Loc.Get("layer.unavailable")));
                stats.Add(Mini(Loc.Get("stat.population") + " · " + Loc.Get("world.estimate"), country.PopulationEstimate > 0 ? Format.Population((long)country.PopulationEstimate) : Loc.Get("layer.unavailable")));
                stats.Add(Mini(Loc.Get("world.continent"), country.Continent));
            }

            sheet.Body.Add(stats);

            if (!simulated)
            {
                var notice = new AlertCard(Loc.Get("world.coming_soon"), Loc.Get("world.coming_soon_body"), StatusLevel.Neutral);
                sheet.Body.Add(notice);
            }

            var actions = new VisualElement();
            actions.AddToClassList("sheet__actions");
            actions.AddToClassList("sheet__actions--grid");
            var view = isPlayer
                ? Buttons.Primary(Loc.Get("world.open_nation"), () =>
                {
                    UI.Sheets.Dismiss();
                    Shell.ShowTab(GameTabId.Nation);
                })
                : Buttons.Primary(Loc.Get("world.view_country"), () => sheet.SnapTo(SheetState.Expanded));
            actions.Add(view);
            actions.Add(Buttons.Secondary(Loc.Get("world.diplomacy"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
            actions.Add(Buttons.Secondary(Loc.Get("world.trade"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
            actions.Add(Buttons.Secondary(Loc.Get("world.compare"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
            sheet.Body.Add(actions);

            var details = new InfoCard(Loc.Get("world.details"));
            if (simulated && definition != null)
            {
                details.AddRow(Loc.Get("stat.government"), Loc.Get(definition.GovernmentKey));
                details.AddRow(Loc.Get("stat.technology"), Loc.Get(CountryTiers.Key(CountryTiers.Technology(state.Technology))));
                details.AddRow(Loc.Get("stat.happiness"), Format.Percent(state.Happiness));
                details.AddRow(Loc.Get("stat.influence"), Format.Index(state.Influence));
                details.AddRow(Loc.Get("resource.energy"), Format.Percent(state.EnergySelfSufficiency * 100));
                details.AddRow(Loc.Get("preview.difficulty"), Loc.Get(CountryDifficultyCalculator.Key(CountryDifficultyCalculator.Calculate(definition))));
                if (!isPlayer)
                {
                    details.AddRow(Loc.Get("world.relationship"), Loc.Get("relationship.pending"));
                }
            }
            else
            {
                details.AddRow(Loc.Get("world.territory"), Loc.Get(TypeKey(country.Type)));
                if (country.SovereignId != country.Id && Context.Map.Catalog.TryGet(country.SovereignId, out var sovereign))
                {
                    details.AddRow(Loc.Get("world.sovereign"), Loc.Get(sovereign.NameKey));
                }

                details.AddRow(Loc.Get("stat.capital"), country.HasCapital ? Loc.Get(country.CapitalKey) : Loc.Get("layer.unavailable"));
                details.AddRow("ISO", country.Id);
            }

            sheet.Body.Add(details);
            sheet.Body.Add(Typography.Caption(Loc.Get("credit.natural_earth"), "sheet__credit"));

            sheet.Dismissed += () =>
            {
                if (_countrySheet == sheet)
                {
                    _countrySheet = null;
                }

                if (!_suppressDeselect && Context.Map.SelectedCountryId == _sheetCountryId)
                {
                    _sheetCountryId = null;
                    Context.Map.Deselect();
                }

                ApplyViewport();
            };
            sheet.StateChanged += _ => ApplyViewport();

            _suppressDeselect = true;
            UI.Sheets.Show(sheet, SheetState.Peek);
            _suppressDeselect = false;
            _countrySheet = sheet;
            sheet.schedule.Execute(ApplyViewport).StartingIn(BottomSheet.AnimationMs + 20);
        }

        private static string TypeKey(MapTerritoryType type)
        {
            switch (type)
            {
                case MapTerritoryType.SovereignCountry: return "type.sovereign_country";
                case MapTerritoryType.Country: return "type.country";
                case MapTerritoryType.Dependency: return "type.dependency";
                case MapTerritoryType.Disputed: return "type.disputed";
                default: return "type.indeterminate";
            }
        }

        private VisualElement Overlay(string overlayName)
        {
            var layer = new VisualElement { name = overlayName, pickingMode = PickingMode.Ignore };
            layer.AddToClassList("map-overlay");
            _viewport.Add(layer);
            return layer;
        }

        private Label HudChip(VisualElement parent, string label, string variant)
        {
            var chip = new VisualElement();
            chip.AddToClassList("hud__chip");
            if (!string.IsNullOrEmpty(variant))
            {
                chip.AddToClassList(variant);
            }

            chip.Add(Typography.Caption(label, "hud__chip-label"));
            var value = Typography.Body(string.Empty, "hud__chip-value");
            chip.Add(value);
            parent.Add(chip);
            return value;
        }

        private static VisualElement Mini(string label, string value)
        {
            var box = new VisualElement();
            box.AddToClassList("mini-stat");
            box.Add(Typography.Caption(label, "mini-stat__label"));
            box.Add(Typography.Body(value, "mini-stat__value"));
            return box;
        }
    }
}
