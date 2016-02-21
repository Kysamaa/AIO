#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Tracker.Properties;
using Color = System.Drawing.Color;
using SharpDX.Direct3D;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace AJS.Utility.Wardsystem
{
    internal enum WardType
    {
        Green,
        Pink,
        Blue,
        Trap
    }

    class WardData
    {
        public int Duration;
        public string ObjectBaseSkinName;
        public int Range;
        public string SpellName;
        public WardType Type;

        public Bitmap Bitmap
        {
            get
            {
                switch (Type)
                {
                    case WardType.Green:
                        return Resources.Minimap_Ward_Green_Enemy;
                    case WardType.Blue:
                        return Resources.Minimap_Ward_Green_Enemy;
                    case WardType.Pink:
                        return Resources.Minimap_Ward_Pink_Enemy;
                    default:
                        return Resources.Minimap_Ward_Green_Enemy;
                }
            }
        }

        public Color Color
        {
            get
            {
                switch (Type)
                {
                    case WardType.Green:
                        return Color.Lime;
                    case WardType.Pink:
                        return Color.Magenta;
                    case WardType.Blue:
                        return Color.Blue;
                    default:
                        return Color.Red;
                }
            }
        }
    }

    class DetectedWard
    {
        private const float _scale = 0.7f;
        private EloBuddy.SDK.Rendering.Circle _defaultCircle;
        private EloBuddy.SDK.Rendering.Circle _defaultCircleFilled;
        private EloBuddy.SDK.Rendering.Sprite _minimapSprite;
        private EloBuddy.SDK.Rendering.Line _missileLine;
        private EloBuddy.SDK.Rendering.Circle _rangeCircle;
        private EloBuddy.SDK.Rendering.Circle _rangeCircleFilled;
        private EloBuddy.SDK.Rendering.Text _timerText;

        public DetectedWard(WardData data,
            Vector3 position,
            int startT,
            Obj_AI_Base wardObject = null,
            bool isFromMissile = false)
        {
            WardData = data;
            Position = position;
            StartT = startT;
            WardObject = wardObject;
            IsFromMissile = isFromMissile;
            CreateRenderObjects();
        }

        public WardData WardData { get; set; }
        public int StartT { get; set; }

        public int Duration
        {
            get { return WardData.Duration; }
        }

        public int EndT
        {
            get { return StartT + Duration; }
        }

        public Vector3 StartPosition { get; set; }
        public Vector3 Position { get; set; }

        public Color Color
        {
            get { return WardData.Color; }
        }

        public int Range
        {
            get { return WardData.Range; }
        }

        public bool IsFromMissile { get; set; }
        public Obj_AI_Base WardObject { get; set; }

        private Vector2 MinimapPosition
        {
            get
            {
                return Drawing.WorldToMinimap(Position) +
                       new Vector2(-WardData.Bitmap.Width / 2f * _scale, -WardData.Bitmap.Height / 2f * _scale);
            }
        }

        public void CreateRenderObjects()
        {
            foreach (var ward in WardTracker.DetectedWards)
            {
                if (ward != null)
                {
                  if (WardTracker.RenderWard.CurrentValue)
                    {
                        Drawing.DrawCircle(ward.Position, ward.Range, Color.Green);
                    }
                    if (Duration != int.MaxValue)
                    {
                        Drawing.DrawText(ward.Position.X, ward.Position.Y + 20, Color.White, "Time:" + ward.Duration);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Ward tracker tracks enemy wards and traps.
    /// </summary>
    class WardTracker
    {
        private static readonly List<WardData> PossibleWards = new List<WardData>();
        public static readonly List<DetectedWard> DetectedWards = new List<DetectedWard>();
        public static Menu Config;
        public static CheckBox RenderWard { get; private set; }

        public static List<Vector3> CrabWardPositions = new List<Vector3>
        {
            new Vector3(4400, 9600, -67),
            new Vector3(10500, 5170, -63)
        };

        public static void WardTrackers()
        {
            //Add the posible wards and their detection type:

            #region PossibleWards

            //Trinkets:
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 1000,
                    ObjectBaseSkinName = "SightWard",
                    Range = 1100,
                    SpellName = "TrinketTotemLvl1",
                    Type = WardType.Green
                });

            PossibleWards.Add(
                new WardData
                {
                    Duration = int.MaxValue,
                    ObjectBaseSkinName = "SightWard",
                    Range = 1100,
                    SpellName = "TrinketOrbLvl3",
                    Type = WardType.Blue
                });

            //Ward items and normal wards:
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 3 * 1000,
                    ObjectBaseSkinName = "SightWard",
                    Range = 1100,
                    SpellName = "SightWard",
                    Type = WardType.Green
                });
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 3 * 1000,
                    ObjectBaseSkinName = "SightWard",
                    Range = 1100,
                    SpellName = "ItemGhostWard",
                    Type = WardType.Green
                });

            //Pinks:
            PossibleWards.Add(
                new WardData
                {
                    Duration = int.MaxValue,
                    ObjectBaseSkinName = "VisionWard",
                    Range = 1100,
                    SpellName = "VisionWard",
                    Type = WardType.Pink
                });

            //Traps
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 4 * 1000,
                    ObjectBaseSkinName = "CaitlynTrap",
                    Range = 300,
                    SpellName = "CaitlynYordleTrap",
                    Type = WardType.Trap
                });
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 10 * 1000,
                    ObjectBaseSkinName = "TeemoMushroom",
                    Range = 212,
                    SpellName = "BantamTrap",
                    Type = WardType.Trap
                });
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 1 * 1000,
                    ObjectBaseSkinName = "ShacoBox",
                    Range = 212,
                    SpellName = "JackInTheBox",
                    Type = WardType.Trap
                });
            PossibleWards.Add(
                new WardData
                {
                    Duration = 60 * 2 * 1000,
                    ObjectBaseSkinName = "Nidalee_Spear",
                    Range = 212,
                    SpellName = "Bushwhack",
                    Type = WardType.Trap
                });
            PossibleWards.Add(
                new WardData
                {
                    Duration = 79 * 1000,
                    ObjectBaseSkinName = "sru_crabward",
                    Range = 1100,
                    SpellName = "",
                    Type = WardType.Green
                });

            #endregion



            //Used for removing the wards that expire:
            Game.OnUpdate += GameOnOnGameUpdate;

            //Used to detect the wards when the unit that places the ward is visible:
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;

            //Used to detect the wards when the unit is not visible but the ward is.
            GameObject.OnCreate += Obj_AI_Base_OnCreate;

            //Used to detect the ward missile when neither the unit or the ward are visible:
            GameObject.OnCreate += ObjSpellMissileOnOnCreate;

            //Process the detected ward objects on the map.
            foreach (var obj in ObjectManager.Get<GameObject>().Where(o => o is Obj_AI_Base))
            {
                Obj_AI_Base_OnCreate(obj, null);
            }
        }

        public static void AttachToMenu()
        {
            Config = MainMenu.AddMenu("Ward", "test");
            RenderWard = Config.Add("enabled", new CheckBox("test"));
        }

        private static void ObjSpellMissileOnOnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid || missile.SpellCaster == null || missile.SpellCaster.IsAlly ||
                missile.SData.Name != "itemplacementmissile" || missile.SpellCaster.IsVisible)
            {
                return;
            }

            var sPos = missile.StartPosition;
            var ePos = missile.EndPosition;
            Core.DelayAction(
                delegate
                {
                    if (
                        DetectedWards.Any(
                            w =>
                                w.Position.To2D().Distance(sPos.To2D(), ePos.To2D(), false, false) < 300 &&
                                Math.Abs(w.StartT - Environment.TickCount) < 2000))
                    {
                        return;
                    }

                    var detectedWard = new DetectedWard(
                        PossibleWards[3], new Vector3(ePos.X, ePos.Y, NavMesh.GetHeightForPosition(ePos.X, ePos.Y)),
                        Environment.TickCount, null, true)
                    {
                        StartPosition = new Vector3(sPos.X, sPos.Y, NavMesh.GetHeightForPosition(sPos.X, sPos.Y))
                    };

                    DetectedWards.Add(detectedWard);
                }, 1000);
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            var wardObject = sender as Obj_AI_Base;

            if (wardObject == null || !wardObject.IsValid || wardObject.IsAlly ||
                sender.Name.ToLower().Contains("corpse"))
            {
                return;
            }

            foreach (var wardData in PossibleWards)
            {
                if (
                    !string.Equals(
                        wardObject.CharData.BaseSkinName, wardData.ObjectBaseSkinName,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var startT = Environment.TickCount - (int) ((wardObject.MaxMana - wardObject.Mana) * 1000);
                DetectedWards.RemoveAll(
                    w =>
                        w.Position.Distance(wardObject.Position) < 200 &&
                        (Math.Abs(w.StartT - startT) < 1000 || wardData.Type != WardType.Green));
                var position = wardObject.Position;

                if (wardData.ObjectBaseSkinName == "sru_crabward")
                {
                    position = CrabWardPositions.FirstOrDefault(p => p.Distance(wardObject.Position) > 1);
                }

                DetectedWards.Add(new DetectedWard(wardData, position, startT, wardObject));
                break;
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var hero = sender as AIHeroClient;

            if (hero == null || sender.IsAlly)
            {
                return;
            }

            foreach (var wardData in PossibleWards)
            {
                if (!string.Equals(args.SData.Name, wardData.SpellName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (wardData.SpellName.Contains("TrinketTotem"))
                {
                    wardData.Duration = 60 + (int) Math.Round(3.5 * (hero.Level - 1));
                }

                var endPosition = ObjectManager.Player.GetPath(args.End).ToList().Last();
                DetectedWards.Add(new DetectedWard(wardData, endPosition, Environment.TickCount));
                break;
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            //Delete the wards that expire or wards that get destroyed:
            DetectedWards.RemoveAll(
                w =>
                    (w.EndT <= Environment.TickCount && w.Duration != int.MaxValue) ||
                    (w.WardObject != null && !w.WardObject.IsValid));
        }
    }
}