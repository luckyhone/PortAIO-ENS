using EnsoulSharp;
using EnsoulSharp.SDK;
using LeagueSharpCommon;
using SharpDX;

namespace iDZed.Utils
{
    internal static class VectorHelper
    {
        public static Vector3[] GetVertices(AIHeroClient target, bool forZhonyas = false) //TODO Zhonyas triangular ult
        {
            Shadow ultShadow = ShadowManager.RShadow;

            if (!ultShadow.Exists)
            {
                return new[] { Vector3.Zero, Vector3.Zero };
            }

            if (forZhonyas)
            {
                Vector2 vertex1 = ObjectManager.Player.ServerPosition.ToVector2() +
                                  Vector2.Normalize(
                                      ObjectManager.Player.ServerPosition.ToVector2() +
                                      Vector2.Normalize(target.ServerPosition.ToVector2() - ultShadow.Position.ToVector2()) *
                                      Zed._spells[SpellSlot.W].Range - ObjectManager.Player.ServerPosition.ToVector2() +
                                      Vector2.Normalize(target.ServerPosition.ToVector2() - ultShadow.Position.ToVector2())
                                          .Perpendicular() * Zed._spells[SpellSlot.W].Range).Perpendicular() *
                                  Zed._spells[SpellSlot.W].Range;
                //
                Vector2 vertex2 = ObjectManager.Player.ServerPosition.ToVector2() +
                                  Vector2.Normalize(
                                      ObjectManager.Player.ServerPosition.ToVector2() +
                                      Vector2.Normalize(target.ServerPosition.ToVector2() - ultShadow.Position.ToVector2())
                                          .Perpendicular() * Zed._spells[SpellSlot.W].Range -
                                      ObjectManager.Player.ServerPosition.ToVector2() +
                                      Vector2.Normalize(target.ServerPosition.ToVector2() - ultShadow.Position.ToVector2()) *
                                      Zed._spells[SpellSlot.W].Range).Perpendicular() * Zed._spells[SpellSlot.W].Range;

                return new[] { vertex1.ToVector3(), vertex2.ToVector3() };
            }

            Vector2 vertex3 = ObjectManager.Player.ServerPosition.ToVector2() +
                              Vector2.Normalize(
                                  target.ServerPosition.ToVector2() - ultShadow.ShadowObject.ServerPosition.ToVector2())
                                  .Perpendicular() * Zed._spells[SpellSlot.W].Range;
            Vector2 vertex4 = ObjectManager.Player.ServerPosition.ToVector2() +
                              Vector2.Normalize(
                                  ultShadow.ShadowObject.ServerPosition.ToVector2() - target.ServerPosition.ToVector2())
                                  .Perpendicular() * Zed._spells[SpellSlot.W].Range;
            return new[] { vertex3.ToVector3(), vertex4.ToVector3() };
        }

        public static Vector3 GetBestPosition(AIHeroClient target, Vector3 firstPosition, Vector3 secondPosition)
        {
            if (Utility.IsWall(firstPosition) && !Utility.IsWall(secondPosition) &&
                secondPosition.Distance(target.ServerPosition) < firstPosition.Distance(target.ServerPosition))
                // if firstposition is a wall and second position isn't
            {
                return secondPosition; //return second position
            }
            if (Utility.IsWall(secondPosition) && !Utility.IsWall(firstPosition) &&
                firstPosition.Distance(target.ServerPosition) < secondPosition.Distance(target.ServerPosition))
                // if secondPosition is a wall and first position isn't
            {
                return firstPosition; // return first position
            }

            return firstPosition;
        }
    }
}