using SwenProject_Arslan.Models;

namespace MyApp.Models;

public interface ICard
{
    string Name { get; set; }
    int Damage { get; set; }
    ElementType ElementType { get; set; }
    int getDamage(ICard otherCard);
}