using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.Interfaces;

public interface ICard
{
    string Name { get; set; }
    int Damage { get; set; }
    ElementType ElementType { get; set; }
    int getDamage(ICard otherCard);
}