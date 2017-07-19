namespace HSRP
{
    public interface IEntity
    {
        string Name { get; set; }

        bool LikesPineappleOnPizza { get; set; }

        AbilitySet Abilities { get; set; }

        int Health { get; set; }
        int MaxHealth { get; set; }
        string Specibus { get; set; }
    }
}