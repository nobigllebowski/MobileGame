namespace Nation.Core.Countries
{
    public enum CountryRegion
    {
        Europe,
        NorthAmerica,
        SouthAmerica,
        Asia,
        MiddleEast,
        Africa,
        Oceania
    }

    public enum GovernmentType
    {
        FederalParliamentaryRepublic,
        FederalPresidentialRepublic,
        PresidentialRepublic,
        SemiPresidentialRepublic,
        ParliamentaryRepublic,
        ConstitutionalMonarchy,
        SocialistRepublic
    }

    public static class CountryEnums
    {
        public static string Key(CountryRegion region)
        {
            switch (region)
            {
                case CountryRegion.Europe: return "region.europe";
                case CountryRegion.NorthAmerica: return "region.north_america";
                case CountryRegion.SouthAmerica: return "region.south_america";
                case CountryRegion.Asia: return "region.asia";
                case CountryRegion.MiddleEast: return "region.middle_east";
                case CountryRegion.Africa: return "region.africa";
                default: return "region.oceania";
            }
        }

        public static string Key(GovernmentType government)
        {
            switch (government)
            {
                case GovernmentType.FederalParliamentaryRepublic: return "government.federal_parliamentary_republic";
                case GovernmentType.FederalPresidentialRepublic: return "government.federal_presidential_republic";
                case GovernmentType.PresidentialRepublic: return "government.presidential_republic";
                case GovernmentType.SemiPresidentialRepublic: return "government.semi_presidential_republic";
                case GovernmentType.ParliamentaryRepublic: return "government.parliamentary_republic";
                case GovernmentType.ConstitutionalMonarchy: return "government.constitutional_monarchy";
                default: return "government.socialist_republic";
            }
        }

        public static bool TryParseRegion(string id, out CountryRegion region)
        {
            switch (id)
            {
                case "europe": region = CountryRegion.Europe; return true;
                case "north_america": region = CountryRegion.NorthAmerica; return true;
                case "south_america": region = CountryRegion.SouthAmerica; return true;
                case "asia": region = CountryRegion.Asia; return true;
                case "middle_east": region = CountryRegion.MiddleEast; return true;
                case "africa": region = CountryRegion.Africa; return true;
                case "oceania": region = CountryRegion.Oceania; return true;
                default: region = CountryRegion.Europe; return false;
            }
        }

        public static bool TryParseGovernment(string id, out GovernmentType government)
        {
            switch (id)
            {
                case "federal_parliamentary_republic": government = GovernmentType.FederalParliamentaryRepublic; return true;
                case "federal_presidential_republic": government = GovernmentType.FederalPresidentialRepublic; return true;
                case "presidential_republic": government = GovernmentType.PresidentialRepublic; return true;
                case "semi_presidential_republic": government = GovernmentType.SemiPresidentialRepublic; return true;
                case "parliamentary_republic": government = GovernmentType.ParliamentaryRepublic; return true;
                case "constitutional_monarchy": government = GovernmentType.ConstitutionalMonarchy; return true;
                case "socialist_republic": government = GovernmentType.SocialistRepublic; return true;
                default: government = GovernmentType.ParliamentaryRepublic; return false;
            }
        }
    }
}
