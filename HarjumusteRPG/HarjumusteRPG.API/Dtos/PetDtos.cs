namespace HarjumusteRPG.API.Dtos;

public class ShopPetDto
{
    public string Key { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool Owned { get; set; }
}

public class BuyPetDto
{
    public string Key { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Coins { get; set; }
}
public class FeedDto
{
    public int CoinsSpent { get; set; }
    public int Coins { get; set; }
}