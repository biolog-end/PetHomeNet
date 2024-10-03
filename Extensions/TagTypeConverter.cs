using PetHome.Models;

public static class TagTypeConverter
{
    public static TagType? ConvertStringToTagType(string tag)
    {
        switch (tag)
        {
            case "Vet":
                return TagType.Vet;
            case "Groomer":
                return TagType.Groomer;
            case "CCTV cameras":
                return TagType.CCTV_Cameras;
            case "Top in your country":
                return TagType.Top_In_Your_Country;
            case "Available discounts":
                return TagType.Available_Discounts;
            case "Free cancelation":
                return TagType.Free_Cancellation;
            case "No prepayment needed":
                return TagType.No_Prepayment_Needed;
            case "Dog handler":
                return TagType.Dog_Handler;
            case "Traditional food":
                return TagType.Traditional_Food;
            case "Special food (hypoallergenic)":
                return TagType.Special_Food_Hypoallergenic;
            case "Regular food":
                return TagType.Regular_Food;
            case "Walking in the yard":
                return TagType.Walking_In_The_Yard;
            case "Walking in own area":
                return TagType.Walking_In_Own_Area;
            case "Walking in the park":
                return TagType.Walking_In_The_Park;
            case "Walking around the city":
                return TagType.Walking_Around_The_City;
            case "Walking in the forest":
                return TagType.Walking_In_The_Forest;
            case "Walking on the beach":
                return TagType.Walking_On_The_Beach;
            case "Near sea":
                return TagType.Near_Sea;
            case "Near mountain":
                return TagType.Near_Mountain;
            case "Near my location":
                return TagType.Near_My_Location;
            default:
                return null;
        }
    }
    public static string ConvertTagTypeToString(TagType tagType)
    {
        switch (tagType)
        {
            case TagType.Vet:
                return "Vet";
            case TagType.Groomer:
                return "Groomer";
            case TagType.CCTV_Cameras:
                return "CCTV cameras";
            case TagType.Top_In_Your_Country:
                return "Top in your country";
            case TagType.Available_Discounts:
                return "Available discounts";
            case TagType.Free_Cancellation:
                return "Free cancelation";
            case TagType.No_Prepayment_Needed:
                return "No prepayment needed";
            case TagType.Dog_Handler:
                return "Dog handler";
            case TagType.Traditional_Food:
                return "Traditional food";
            case TagType.Special_Food_Hypoallergenic:
                return "Special food (hypoallergenic)";
            case TagType.Regular_Food:
                return "Regular food";
            case TagType.Walking_In_The_Yard:
                return "Walking in the yard";
            case TagType.Walking_In_Own_Area:
                return "Walking in own area";
            case TagType.Walking_In_The_Park:
                return "Walking in the park";
            case TagType.Walking_Around_The_City:
                return "Walking around the city";
            case TagType.Walking_In_The_Forest:
                return "Walking in the forest";
            case TagType.Walking_On_The_Beach:
                return "Walking on the beach";
            case TagType.Near_Sea:
                return "Near sea";
            case TagType.Near_Mountain:
                return "Near mountain";
            case TagType.Near_My_Location:
                return "Near my location";
            default:
                return null;
        }
    }
}

