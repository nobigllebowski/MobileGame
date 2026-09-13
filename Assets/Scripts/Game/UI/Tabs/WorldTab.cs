using Nation.Core.Countries;
using Nation.Core.Models;
using Nation.Core.Nation;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>Main gameplay space: HUD, time controls, and the pannable placeholder world view.</summary>
    public sealed class WorldTab : GameTab
    {
        private const float MapAspect = 360f / 142f;

        private Label _date;
        private Label _treasury;
        private Label _gdp;
        private Label _population;
        private Label _speedLabel;
        private TimeControls _time;
        private WorldMapView _map;
        private VisualElement _viewport;
        private bool _panning;
        private int _pointerId = -1;
        private Vector2 _panStart;
        private float _mapStartLeft;

        public WorldTab(GameContext context, GameShellScreen shell) : base(context, shell, "tab-world")
        {
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
            _map = new WorldMapView(Context.MapData, c => Loc.Get(c.NameKey));
            _map.CountryTapped += OnCountryTapped;
            _viewport.Add(_map);
            _viewport.RegisterCallback<GeometryChangedEvent>(_ => LayoutMap());
            _viewport.RegisterCallback<PointerDownEvent>(OnPanStart);
            _viewport.RegisterCallback<PointerMoveEvent>(OnPanMove);
            _viewport.RegisterCallback<PointerUpEvent>(OnPanEnd);
            _viewport.RegisterCallback<PointerCancelEvent>(evt => EndPan());
            Add(_viewport);

            var timeBar = new VisualElement();
            timeBar.AddToClassList("time-bar");
            _speedLabel = Typography.Caption(string.Empty, "time-bar__label");
            timeBar.Add(_speedLabel);
            _time = new TimeControls();
            _time.SpeedRequested += speed => Session.SetSpeed(speed);
            timeBar.Add(_time);
            Add(timeBar);

            _map.SetCountries(Context.Countries.Playable, Session.PlayerCountryId);
            Refresh();
            OnSpeedChanged(Session.Speed);
        }

        public override void OnShow()
        {
            if (IsBuilt)
            {
                Refresh();
                OnSpeedChanged(Session.Speed);
            }
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

        private void Refresh()
        {
            var country = Session.PlayerCountry;
            _date.text = Format.Date(Session.World.CurrentDate);
            _treasury.text = Format.Money(country.Treasury);
            _gdp.text = Format.Money(country.Gdp);
            _population.text = Format.Population(country.Population);
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

        private void LayoutMap()
        {
            var height = _viewport.resolvedStyle.height;
            var viewWidth = _viewport.resolvedStyle.width;
            if (height <= 0 || viewWidth <= 0)
            {
                return;
            }

            var width = Mathf.Max(viewWidth, height * MapAspect);
            _map.style.width = width;
            _map.style.height = height;

            var capital = Session.PlayerDefinition;
            var focusX = (float)(Nation.Core.Map.MapProjection.X(capital.CapitalLongitude) * width);
            _map.style.left = ClampLeft(viewWidth * 0.5f - focusX, width, viewWidth);
        }

        private static float ClampLeft(float left, float mapWidth, float viewWidth)
        {
            var min = viewWidth - mapWidth;
            return Mathf.Clamp(left, Mathf.Min(min, 0f), 0f);
        }

        private void OnPanStart(PointerDownEvent evt)
        {
            _panning = true;
            _pointerId = evt.pointerId;
            _panStart = evt.position;
            _mapStartLeft = _map.resolvedStyle.left;
            _viewport.CapturePointer(_pointerId);
        }

        private void OnPanMove(PointerMoveEvent evt)
        {
            if (!_panning || evt.pointerId != _pointerId)
            {
                return;
            }

            var dx = evt.position.x - _panStart.x;
            _map.style.left = ClampLeft(_mapStartLeft + dx, _map.resolvedStyle.width, _viewport.resolvedStyle.width);
        }

        private void OnPanEnd(PointerUpEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                EndPan();
            }
        }

        private void EndPan()
        {
            if (_panning && _viewport.HasPointerCapture(_pointerId))
            {
                _viewport.ReleasePointer(_pointerId);
            }

            _panning = false;
            _pointerId = -1;
        }

        private void OnCountryTapped(CountryDefinition country)
        {
            var isPlayer = country.Id == Session.PlayerCountryId;
            var state = Session.World.GetCountry(country.Id);

            var sheet = new BottomSheet();
            sheet.SetSnapPoints(0.36f, 0.6f, 0.88f);

            var header = new VisualElement();
            header.AddToClassList("sheet__country");
            header.Add(new FlagElement(country.Flag, "flag--sheet"));
            var titles = new VisualElement();
            titles.AddToClassList("sheet__country-text");
            titles.Add(Typography.Heading(Loc.Get(country.NameKey), "sheet__country-name"));
            titles.Add(Typography.Caption(Loc.Get(country.CapitalKey) + "  ·  " + Loc.Get(country.RegionKey), "sheet__country-meta"));
            header.Add(titles);
            if (isPlayer)
            {
                header.Add(new Badge(Loc.Get("world.your_nation"), StatusLevel.Positive));
            }
            sheet.Header.Add(header);

            var stats = new VisualElement();
            stats.AddToClassList("sheet__stats");
            stats.Add(Mini(Loc.Get("stat.gdp"), Format.Money(state.Gdp)));
            stats.Add(Mini(Loc.Get("stat.population"), Format.Population(state.Population)));
            stats.Add(Mini(Loc.Get("stat.influence"), Format.Index(state.Influence)));
            stats.Add(Mini(Loc.Get("stat.stability"), Format.Percent(state.Stability)));
            sheet.Body.Add(stats);

            var actions = new VisualElement();
            actions.AddToClassList("sheet__actions");
            if (isPlayer)
            {
                actions.Add(Buttons.Primary(Loc.Get("world.open_nation"), () =>
                {
                    UI.Sheets.Dismiss();
                    Shell.ShowTab(GameTabId.Nation);
                }));
            }
            else
            {
                actions.Add(Buttons.Secondary(Loc.Get("world.diplomacy"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
                actions.Add(Buttons.Secondary(Loc.Get("world.trade"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
            }
            sheet.Body.Add(actions);

            var details = new InfoCard(Loc.Get("world.details"));
            details.AddRow(Loc.Get("stat.technology"), Loc.Get(CountryTiers.Key(CountryTiers.Technology(state.Technology))));
            details.AddRow(Loc.Get("stat.happiness"), Format.Percent(state.Happiness));
            details.AddRow(Loc.Get("resource.energy"), Format.Percent(state.EnergySelfSufficiency * 100));
            details.AddRow(Loc.Get("preview.difficulty"), Loc.Get(CountryDifficultyCalculator.Key(CountryDifficultyCalculator.Calculate(country))));
            details.AddRow(Loc.Get("stat.government"), Loc.Get(country.GovernmentKey));
            sheet.Body.Add(details);

            UI.Sheets.Show(sheet, SheetState.Peek);
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
