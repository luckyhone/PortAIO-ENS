using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;

namespace Challenger_Series.Utils.Logic
{
    public class PositionSaver
    {
        private PositionSaverCore _core;
        private MenuKeyBind _saveKey;
        private MenuKeyBind _deleteKey;
        private MenuKeyBind _deleteOneKey;
        private MenuBool _isEnabled;
        private Spell _spellToUse;
        public PositionSaver(Menu menu, Spell spellToUse)
        {
            _core = new PositionSaverCore();
            _isEnabled = menu.Add(new MenuBool("positionsaverenabled", "Auto use in custom positions", true));
            _saveKey = menu.Add(new MenuKeyBind("positionsaversavekey", "Save cursor pos as custom pos!", Keys.I, KeyBindType.Press));
            _deleteOneKey = menu.Add(new MenuKeyBind("positionsaverdeleteone", "Delete cursor position", Keys.J, KeyBindType.Press));
            _deleteKey = menu.Add(new MenuKeyBind("positionsaverpurge", "Delete all positions", Keys.Delete, KeyBindType.Press));
            _spellToUse = spellToUse;
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            if (_isEnabled.Enabled)
            {
                if (_saveKey.Active)
                {
                    if (!_core.Positions.Any(pos => pos.Distance(Game.CursorPos) < 100))
                    {
                        _core.SavePosition(Game.CursorPos);
                        _core.Positions.Add(Game.CursorPos);
                    }
                }
                if (_deleteKey.Active)
                {
                    _core.PurgeAllPositions();
                }
                if (_deleteOneKey.Active)
                {
                    _core.RemovePosition(Game.CursorPos);
                }
                if (_core.Positions.Any())
                {
                    foreach (var savedLocation in _core.Positions.Where(pos => pos.Distance(ObjectManager.Player.Position) < 4000))
                    {
                        CircleRender.Draw(savedLocation, 60, savedLocation.Distance(ObjectManager.Player.Position) < _spellToUse.Range ? Color.Gold.ToSharpDxColor() : Color.White.ToSharpDxColor());
                    }
                    if (_spellToUse.IsReady())
                    {
                        foreach (var position in _core.Positions.Where(pos => pos.Distance(ObjectManager.Player.Position) < _spellToUse.Range))
                        {
                            if (position.IsValid() && !GameObjects.AllyMinions.Any(m => m.Position.Distance(position) < 100))
                            {
                                _spellToUse.Cast(position);
                            }
                        }
                    }
                }
            }
        }
    }
}