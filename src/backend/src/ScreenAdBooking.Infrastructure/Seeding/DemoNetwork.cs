using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Seeding;

/// <summary>
/// Fictional screen network in France and Belgium. Cities are real; venues, businesses and street
/// addresses are made up, placed a few hundred metres from the city centre.
/// </summary>
public static class DemoNetwork
{
    public static IReadOnlyList<DemoCountry> Countries { get; } =
    [
        new("FR", "France"),
        new("BE", "Belgium"),
    ];

    public static IReadOnlyList<DemoCity> Cities { get; } =
    [
        new("FR", "Île-de-France", "Paris", 48.8566, 2.3522),
        new("FR", "Île-de-France", "Boulogne-Billancourt", 48.8397, 2.2399),
        new("FR", "Auvergne-Rhône-Alpes", "Lyon", 45.7640, 4.8357),
        new("FR", "Auvergne-Rhône-Alpes", "Grenoble", 45.1885, 5.7245),
        new("FR", "Provence-Alpes-Côte d'Azur", "Marseille", 43.2965, 5.3698),
        new("FR", "Provence-Alpes-Côte d'Azur", "Nice", 43.7102, 7.2620),
        new("FR", "Occitanie", "Toulouse", 43.6047, 1.4442),
        new("FR", "Occitanie", "Montpellier", 43.6108, 3.8767),
        new("BE", "Brussels-Capital Region", "Brussels", 50.8503, 4.3517),
        new("BE", "Flanders", "Antwerp", 51.2194, 4.4025),
        new("BE", "Flanders", "Ghent", 51.0543, 3.7174),
        new("BE", "Wallonia", "Liège", 50.6326, 5.5797),
        new("BE", "Wallonia", "Namur", 50.4674, 4.8718),
    ];

    public static IReadOnlyList<DemoVenue> Venues { get; } =
    [
        new("Paris", "Canal Fresh Market", VenueType.Supermarket, Footfall.High, LocationType.CityCentre, "14 Rue des Lilas", 0.004, 0.006, 4, 320),
        new("Paris", "North Station Concourse", VenueType.Station, Footfall.High, LocationType.TransportHub, "1 Place de la Gare", 0.024, 0.003, 6, 900),
        new("Boulogne-Billancourt", "Bridge Pharmacy", VenueType.Pharmacy, Footfall.Medium, LocationType.Suburb, "7 Avenue des Tilleuls", -0.003, 0.002, 2, 60),
        new("Boulogne-Billancourt", "Riverside Fitness", VenueType.Gym, Footfall.Medium, LocationType.Suburb, "22 Quai du Fleuve", 0.002, -0.005, 3, 110),
        new("Lyon", "Two Rivers Gallery", VenueType.ShoppingMall, Footfall.High, LocationType.ShoppingCentre, "5 Rue du Commerce", 0.003, 0.021, 8, 750),
        new("Lyon", "Silk Hill Bakery", VenueType.Bakery, Footfall.Medium, LocationType.CityCentre, "3 Rue des Boulangers", 0.010, 0.001, 1, 45),
        new("Grenoble", "Summit Cinema", VenueType.Cinema, Footfall.Medium, LocationType.CityCentre, "9 Boulevard des Cimes", -0.002, 0.003, 3, 210),
        new("Grenoble", "Valley Fresh Market", VenueType.Supermarket, Footfall.Medium, LocationType.Suburb, "40 Avenue des Peupliers", -0.015, -0.010, 2, 180),
        new("Marseille", "Old Harbour Brasserie", VenueType.Restaurant, Footfall.High, LocationType.CityCentre, "2 Quai du Port", -0.001, -0.004, 2, 140),
        new("Marseille", "Hilltop Station Hall", VenueType.Station, Footfall.High, LocationType.TransportHub, "1 Square de la Gare", 0.006, 0.010, 5, 820),
        new("Nice", "Seafront Pharmacy", VenueType.Pharmacy, Footfall.Medium, LocationType.CityCentre, "11 Rue des Oliviers", -0.003, -0.002, 1, 55),
        new("Nice", "Riviera Mall", VenueType.ShoppingMall, Footfall.High, LocationType.ShoppingCentre, "60 Route du Littoral", -0.020, -0.045, 6, 600),
        new("Toulouse", "Pink Brick Bakery", VenueType.Bakery, Footfall.High, LocationType.CityCentre, "8 Rue des Briques", 0.001, 0.002, 1, 70),
        new("Toulouse", "Willow Fitness Club", VenueType.Gym, Footfall.Low, LocationType.Suburb, "15 Chemin des Saules", -0.030, 0.020, 2, 80),
        new("Montpellier", "Square Cinema", VenueType.Cinema, Footfall.High, LocationType.CityCentre, "4 Place du Théâtre", 0.000, 0.003, 4, 260),
        new("Montpellier", "Pine Grove Market", VenueType.Supermarket, Footfall.Low, LocationType.Suburb, "33 Avenue des Pins", 0.025, -0.015, 2, 120),
        new("Brussels", "Central Station Arcade", VenueType.Station, Footfall.High, LocationType.TransportHub, "3 Galerie de la Gare", -0.005, 0.005, 6, 950),
        new("Brussels", "Garden Square Bakery", VenueType.Bakery, Footfall.Medium, LocationType.CityCentre, "12 Rue des Pâtissiers", -0.010, 0.003, 1, 50),
        new("Antwerp", "Harbour Lights Shopping Centre", VenueType.ShoppingMall, Footfall.High, LocationType.ShoppingCentre, "25 Kaailaan", 0.010, -0.010, 7, 680),
        new("Antwerp", "Market Street Pharmacy", VenueType.Pharmacy, Footfall.High, LocationType.CityCentre, "6 Winkelstraat", -0.002, 0.008, 1, 75),
        new("Ghent", "Canal Side Bistro", VenueType.Restaurant, Footfall.Medium, LocationType.CityCentre, "5 Waterkant", 0.001, 0.004, 2, 90),
        new("Ghent", "Park Lane Fitness", VenueType.Gym, Footfall.Medium, LocationType.Suburb, "48 Parklaan", -0.025, 0.015, 3, 100),
        new("Liège", "Boatmen Cinema", VenueType.Cinema, Footfall.Medium, LocationType.CityCentre, "10 Quai des Bateliers", 0.004, 0.002, 3, 190),
        new("Liège", "Station Square Market", VenueType.Supermarket, Footfall.High, LocationType.TransportHub, "2 Place de la Station", -0.008, -0.010, 3, 400),
        new("Namur", "Rampart Bakery", VenueType.Bakery, Footfall.Low, LocationType.CityCentre, "4 Rue du Rempart", -0.003, -0.002, 1, 30),
        new("Namur", "Meadow Pharmacy", VenueType.Pharmacy, Footfall.Low, LocationType.Suburb, "19 Rue des Prés", 0.020, 0.025, 1, 35),
    ];
}

public sealed record DemoCountry(string Code, string Name);

public sealed record DemoCity(string CountryCode, string Region, string Name, double Latitude, double Longitude);

/// <param name="LatitudeOffset">Degrees from the city centre.</param>
/// <param name="LongitudeOffset">Degrees from the city centre.</param>
public sealed record DemoVenue(
    string City,
    string Name,
    VenueType Type,
    Footfall Footfall,
    LocationType LocationType,
    string Address,
    double LatitudeOffset,
    double LongitudeOffset,
    int ScreenCount,
    int AverageAudience);
