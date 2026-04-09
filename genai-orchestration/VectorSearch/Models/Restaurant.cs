using Microsoft.Extensions.VectorData;

namespace VectorSearch.Models;

public class Restaurant
{
    [VectorStoreKey]
    public int Key { get; set; }

    [VectorStoreData]
    public string Name { get; set; }

    [VectorStoreData]
    public string Description { get; set; }

    [VectorStoreData]
    public string CuisineType { get; set; }

    [VectorStoreData]
    public string PriceRange { get; set; }

    [VectorStoreData]
    public string Location { get; set; }

    [VectorStoreVector(
        Dimensions: 384,
        DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

public static class RestaurantData
{
    public static List<Restaurant> Restaurants =>
    [
        new Restaurant
        {
            Key = 0,
            Name = "Sakura Japanese Cuisine",
            Description = "Authentic Japanese restaurant serving fresh sushi, sashimi, and traditional hot dishes like ramen and donburi in an elegant setting.",
            CuisineType = "Japanese",
            PriceRange = "$$",
            Location = "Downtown"
        },
        new Restaurant
        {
            Key = 1,
            Name = "Bella Italia Ristorante",
            Description = "Family-owned Italian eatery featuring homemade pasta, wood-fired pizzas, and classic dishes like lasagna and chicken parmigiana.",
            CuisineType = "Italian",
            PriceRange = "$$",
            Location = "North End"
        },
        new Restaurant
        {
            Key = 2,
            Name = "El Paraiso Mexican Grill",
            Description = "Vibrant taqueria offering authentic tacos, burritos, enchiladas, and fresh guacamole with a full margarita bar.",
            CuisineType = "Mexican",
            PriceRange = "$",
            Location = "South Side"
        },
        new Restaurant
        {
            Key = 3,
            Name = "Le Bistro Français",
            Description = "Upscale French restaurant serving escargot, coq au vin, and crème brûlée in a romantic candlelit atmosphere.",
            CuisineType = "French",
            PriceRange = "$$$",
            Location = "Uptown"
        },
        new Restaurant
        {
            Key = 4,
            Name = "Spice Garden Indian Kitchen",
            Description = "Warm, cozy spot for traditional Indian curries, biryani, tandoori dishes, and freshly baked naan bread.",
            CuisineType = "Indian",
            PriceRange = "$$",
            Location = "East Village"
        },
        new Restaurant
        {
            Key = 5,
            Name = "Burger Joint Classic",
            Description = "Casual diner serving gourmet burgers, crispy fries, milkshakes, and comfort food favorites in a retro setting.",
            CuisineType = "American",
            PriceRange = "$",
            Location = "Westside"
        }
    ];
}
