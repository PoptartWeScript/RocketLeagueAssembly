using System;

using System.Collections.Generic;
using System.Linq;

using SharpDX;

using WeScriptWrapper;
using WeScript.SDK.UI;
using WeScript.SDK.UI.Components;












namespace RocketLeague
{
    public static class Program
    {
        public static IntPtr processHandle = IntPtr.Zero; 
        public static bool gameProcessExists = false; 
        public static bool isWow64Process = false; 
        public static bool isGameOnTop = false; 
        public static bool isOverlayOnTop = false; 
        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF; 
        public static Vector2 wndMargins = new Vector2(0, 0); 
        public static Vector2 wndSize = new Vector2(0, 0); 
        public static IntPtr GameBase = IntPtr.Zero;
        public static IntPtr GameSize = IntPtr.Zero;
        public static IntPtr GameEvent = IntPtr.Zero;
        public static Vector2 GameCenterPos = new Vector2(0, 0);
        public static bool wasWPressed = false;
        public static bool wasAPressed = false;
        public static bool wasDPressed = false;
        public static bool wasLShiftPressed = false;
        public static bool wasRMousePressed = false;
        public static bool wasFPressed = false;



        public static Menu RootMenu { get; private set; }
        public static Menu VisualsMenu { get; private set; }

        

       
        

       

        

        class Components
        {
            public static readonly MenuKeyBind MainAssemblyToggle = new MenuKeyBind("mainassemblytoggle", "Toggle the whole assembly effect by pressing key:", WeScriptWrapper.VirtualKeyCode.Delete, KeybindType.Toggle, true);
            public static class VisualsComponent
            {
                public static readonly MenuBool DrawTheVisuals = new MenuBool("drawthevisuals", "Enable all of the Visuals", true);

                public static readonly MenuBool DrawBoostTimer = new MenuBool("drawtext", "Draw Boost Timer", true);

                public static readonly MenuBool DrawBallESP = new MenuBool("drawballesp", "Draw BALL ESP", true);

                public static readonly MenuBool TeamGoalESP = new MenuBool("teamesp", "Draw TeamGoal ESP", true);

                public static readonly MenuBool EnemyGoalESP = new MenuBool("enemygoalesp", "Draw EnemyGoal ESP", true);

                public static readonly MenuBool BallToGoalESP = new MenuBool("balltogoalesp", "Draw Ball2Goal ESP", true);

                public static readonly MenuKeyBind BallChase = new MenuKeyBind("AimtoBall", "Hold Hotkey to Force Car to Ball", WeScriptWrapper.VirtualKeyCode.F, KeybindType.Hold, active:false );

                public static readonly MenuBool AutoJump = new MenuBool("Jump", "Jump Into Ball", true);

                public static readonly MenuBool AutoKickOff = new MenuBool("Kickoff", "AutoKickoff", true);

                public static readonly MenuBool AimAtGoal = new MenuBool("AimAssist", "AimAtGoal", true);

                public static readonly MenuBool MatchBallSpeed = new MenuBool("MatchSpeed", "MatchBallSpeed", true);

                public static readonly MenuBool AutoScan = new MenuBool("Rescan", "Scan during new match", true);

                public static readonly MenuSlider normalMaxSpeed = new MenuSlider("normalMaxSpeed", "normalMaxSpeed", 500, 95, 500);


            }
        }


        public static void InitializeMenu()
        {
            VisualsMenu = new Menu("visualsmenu", "Visuals Menu")
            {
                Components.VisualsComponent.DrawTheVisuals,

                Components.VisualsComponent.DrawBoostTimer,

                Components.VisualsComponent.DrawBallESP,

                Components.VisualsComponent.TeamGoalESP,

                Components.VisualsComponent.EnemyGoalESP,

                Components.VisualsComponent.BallToGoalESP,
                
                Components.VisualsComponent.BallChase,

                Components.VisualsComponent.AutoJump,

                Components.VisualsComponent.AutoKickOff,

                Components.VisualsComponent.AimAtGoal,

                Components.VisualsComponent.AutoScan,








            };


            RootMenu = new Menu("RocketLeague", "WeScript.app RocketLeague Assembly", true)
            {
                
                VisualsMenu,
                
            };
            RootMenu.Attach();
        }


        static void Main(string[] args)
        {
            Console.WriteLine("WeScript.app RocketLeague Assembly By Poptart && GameHackerPM 0.1.6 BETA Loaded!");
            bool returnedbool1 = WeScript.SDK.Utils.VIP.IsTopicContentUnlocked("/191-rocket-league-beta-v017/");

            if(returnedbool1 == false)
            {
                Console.WriteLine("Thank you for being a VIP Member");
                Console.WriteLine(" I SEE YOU Tomić");
                InitializeMenu();
                Renderer.OnRenderer += OnRenderer;
                Memory.OnTick += OnTick;
                
            }
            else
                if (returnedbool1 == false)
            {
                Console.WriteLine("NOT A VIP MEMBER!!!");
            }


            



        }

        private static void OnTick(int counter, EventArgs args)
        {
            if (processHandle == IntPtr.Zero) 
            {
                var wndHnd = Memory.FindWindowName("Rocket League (64-bit, DX11, Cooked)"); 
                if (wndHnd != IntPtr.Zero) 
                {
                    var calcPid = Memory.GetPIDFromHWND(wndHnd); 
                    if (calcPid > 0) 
                    {
                        processHandle = Memory.OpenProcess(PROCESS_ALL_ACCESS, calcPid); 
                        if (processHandle != IntPtr.Zero)
                        {
                            
                            isWow64Process = Memory.IsProcess64Bit(processHandle);
                            
                        }
                    }
                }
            }
            else 
            {
                var wndHnd = Memory.FindWindowName("Rocket League (64-bit, DX11, Cooked)");
                if (wndHnd != IntPtr.Zero) 
                {
                    
                    gameProcessExists = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);
                    GameCenterPos = new Vector2(wndSize.X / 2 + wndMargins.X, wndSize.Y / 2 + wndMargins.Y);
                    isOverlayOnTop = Overlay.IsOnTop();
                    GameBase = Memory.GetModule(processHandle, null, isWow64Process);
                }
                else 
                {
                    Memory.CloseHandle(processHandle); 
                    processHandle = IntPtr.Zero; 
                    gameProcessExists = false;
                }
            }
        }
        
        public static List<long> BoostsObjects = new List<long>();
        private static Dictionary<long, DateTime> BoostsTimers = new Dictionary<long, DateTime>();
        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!gameProcessExists) return;
            if ((!isGameOnTop) && (!isOverlayOnTop)) return;

            if (GameEvent == IntPtr.Zero)
            {
                Console.WriteLine($"[{DateTime.Now}] Scanning.");
                string sig = "08 64 ?? ?? ?? 7F 00 00 ?? ?? ?? ?? ?? ?? 00 00 04 00 10 10 01 00 00 02 ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00";
                GameEvent = Memory.FindSignatureBase(processHandle, GameBase, GameSize, sig);
                Console.WriteLine("Scan Completed! Found GameEvent!");
                Console.WriteLine("MAKE SURE TO PRESS F5 WHEN ENTERING A NEW GAME!!");
                
            }
            



            var GameEngine = Memory.ReadPointer(processHandle, (IntPtr)GameBase.ToInt64() + 0x24221F0, isWow64Process);
            var LocalPlayersArray = Memory.ReadPointer(processHandle, (IntPtr)GameEngine.ToInt64() + 0x760, isWow64Process);

            var LocalPlayer = Memory.ReadPointer(processHandle, (IntPtr)LocalPlayersArray.ToInt64(), isWow64Process);
            var PlayerController = Memory.ReadPointer(processHandle, (IntPtr)LocalPlayer.ToInt64() + 0x0078, isWow64Process);
            var WorldInfo = Memory.ReadPointer(processHandle, (IntPtr)PlayerController.ToInt64() + 0x0130, isWow64Process);
            var WorldGravityZ = Memory.ReadFloat(processHandle, (IntPtr)WorldInfo.ToInt64() + 0x061C);
            var DefaultGravityZ = Memory.ReadFloat(processHandle, (IntPtr)WorldInfo.ToInt64() + 0x0620);
            var GlobalGravityZ = Memory.ReadFloat(processHandle, (IntPtr)WorldInfo.ToInt64() + 0x0624);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///

            //if (Components.VisualsComponent.AutoScan.Enabled)
            //{
            //    var GameTimeInSeconds = Memory.ReadInt64(processHandle, (IntPtr)GameEvent.ToInt64() + 0x07D8);
               
            //    if (GameTimeInSeconds > 300)
            //    {
            //        Console.WriteLine($"[{DateTime.Now}] Scanning.");
            //        string sig = "C0 1F ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 04 00 10 10 01 00 00 02 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF ?? ?? ?? ?? FF FF FF FF ?? ?? ?? ?? ?? ?? ?? ?? F9 EC 00 00 ?? 00 00 00";
            //        GameEvent = Memory.FindSignatureBase(processHandle, GameBase, GameSize, sig);
            //        Console.WriteLine("Scan Completed! Found GameEvent!");
            //        Console.WriteLine("MAKE SURE TO PRESS F5 WHEN ENTERING A NEW GAME!!");
            //    }

            //    if (GameTimeInSeconds <= 0)
            //    {
            //        Console.WriteLine($"[{DateTime.Now}] Scanning.");
            //        string sig = "C0 1F ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 04 00 10 10 01 00 00 02 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF ?? ?? ?? ?? FF FF FF FF ?? ?? ?? ?? ?? ?? ?? ?? F9 EC 00 00 ?? 00 00 00";
            //        GameEvent = Memory.FindSignatureBase(processHandle, GameBase, GameSize, sig);
            //        Console.WriteLine("Scan Completed! Found GameEvent!");
            //        Console.WriteLine("MAKE SURE TO PRESS F5 WHEN ENTERING A NEW GAME!!");
            //    }
            //}

            ///////////////////////////////////////////NEW UPDATE INFO///////////////////////////////////////////////////////////////////////

            var AController = Memory.ReadPointer(processHandle, (IntPtr)WorldInfo.ToInt64() + 0x0640, isWow64Process);
            var APawn = Memory.ReadPointer(processHandle, (IntPtr)AController.ToInt64() + 0x0280, isWow64Process);
            var APlayerReplicationInfo = Memory.ReadPointer(processHandle, (IntPtr)AController.ToInt64() + 0x0288, isWow64Process);
            var ATeamInfo = Memory.ReadPointer(processHandle, (IntPtr)APlayerReplicationInfo.ToInt64() + 0x02B0, isWow64Process);

            var ScoreNum = Memory.ReadInt32(processHandle, (IntPtr)ATeamInfo.ToInt64() + 0x027C);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            var PlayerCamera = Memory.ReadPointer(processHandle, (IntPtr)(PlayerController.ToInt64() + 0x0480), isWow64Process);



            var Location = Memory.ReadVector3(processHandle, (IntPtr)PlayerCamera.ToInt64() + 0x0090);
            var LastCamFov = Memory.ReadFloat(processHandle, (IntPtr)PlayerCamera.ToInt64() + 0x0278);
            var Pitch = Memory.ReadInt32(processHandle, (IntPtr)PlayerCamera.ToInt64() + 0x009C);
            var Yaw = Memory.ReadInt32(processHandle, (IntPtr)PlayerCamera.ToInt64() + 0x009C + 0x04);
            var Roll = Memory.ReadInt32(processHandle, (IntPtr)PlayerCamera.ToInt64() + 0x009C + 0x08);



            ////////////////////////////////////////BALL INFO//////////////////////////////////////////////////////////////

            //var GameTimeInSeconds = Memory.ReadInt64(processHandle, (IntPtr)GameEvent.ToInt64() + 0x07D8);
            

            var GameBalls = Memory.ReadPointer(processHandle, (IntPtr)GameEvent.ToInt64() + 0x0840, isWow64Process);
            var Ball = Memory.ReadPointer(processHandle, (IntPtr)GameBalls.ToInt64() + 0x0000, isWow64Process);
            var BallLocation = Memory.ReadVector3(processHandle, (IntPtr)Ball.ToInt64() + 0x0090);
            var BallVelocity = Memory.ReadVector3(processHandle, (IntPtr)Ball.ToInt64() + 0x01A8);


            var Throttle = Memory.ReadFloat(processHandle, (IntPtr)PlayerController.ToInt64() + 0x0958);
            var Steer = Memory.ReadFloat(processHandle, (IntPtr)PlayerController.ToInt64() + 0x095C);
            var Car = Memory.ReadPointer(processHandle, (IntPtr)PlayerController.ToInt64() + 0x0968, isWow64Process);
            var CarLocation = Memory.ReadVector3(processHandle, (IntPtr)Car.ToInt64() + 0x0090);
            var CarRotation = Memory.ReadVector3(processHandle, (IntPtr)Car.ToInt64() + 0x009C);
            var CarYaw = Memory.ReadInt32(processHandle, (IntPtr)Car.ToInt64() + 0x009C + 0x04);
            var CarVelocity = Memory.ReadVector3(processHandle, (IntPtr)Car.ToInt64() + 0x01A8);
            var CarPickUp = Memory.ReadPointer(processHandle, (IntPtr)Car.ToInt64() + 0x09A8, isWow64Process);
            var AttachedPickup = Memory.ReadPointer(processHandle, (IntPtr)CarPickUp.ToInt64() + 0x02C0, isWow64Process);
            var PickUp = Memory.ReadString(processHandle, (IntPtr)AttachedPickup.ToInt64() + 0x0000, isWow64Process);
            //Console.WriteLine(PickUp);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////GOAL STUFF HERE/////////////////////////////////////////////////////////////////////
            var UGoal = Memory.ReadPointer(processHandle, (IntPtr)GameEvent.ToInt64() + 0x0860, isWow64Process);
            var TeamGoal = Memory.ReadPointer(processHandle, (IntPtr)UGoal.ToInt64() + 0x0000, isWow64Process);
            var TeamGoalLocation = Memory.ReadVector3(processHandle, (IntPtr)TeamGoal.ToInt64() + 0x0120);
            var TeamGoalLocationWorldCenter = Memory.ReadVector3(processHandle, (IntPtr)TeamGoal.ToInt64() + 0x0168);
            var EnemyGoal = Memory.ReadPointer(processHandle, (IntPtr)UGoal.ToInt64() + 0x0008, isWow64Process);
            var EnemyGoalLocation = Memory.ReadVector3(processHandle, (IntPtr)EnemyGoal.ToInt64() + 0x0120);


            



            ////////////////////////////////Putting Boost things here!!///////////////////////////////////////////////////


            var GameShare = Memory.ReadPointer(processHandle, (IntPtr)(WorldInfo.ToInt64() + 0x0AF0), isWow64Process);

            var BoostA = Memory.ReadPointer(processHandle, (IntPtr)(GameShare.ToInt64() + 0x0078), isWow64Process);

            var Boost1 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0000), isWow64Process);
            var Pill1 = Memory.ReadVector3(processHandle, (IntPtr)Boost1.ToInt64() + 0x0090);

            var Boost2 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0008), isWow64Process);
            var Pill2 = Memory.ReadVector3(processHandle, (IntPtr)Boost2.ToInt64() + 0x0090);

            var Boost3 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0010), isWow64Process);
            var Pill3 = Memory.ReadVector3(processHandle, (IntPtr)Boost3.ToInt64() + 0x0090);

            var Boost4 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0018), isWow64Process);
            var Pill4 = Memory.ReadVector3(processHandle, (IntPtr)Boost4.ToInt64() + 0x0090);

            var Boost5 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0020), isWow64Process);
            var Pill5 = Memory.ReadVector3(processHandle, (IntPtr)Boost5.ToInt64() + 0x0090);

            var Boost6 = Memory.ReadPointer(processHandle, (IntPtr)(BoostA.ToInt64() + 0x0028), isWow64Process);
            var Pill6 = Memory.ReadVector3(processHandle, (IntPtr)Boost6.ToInt64() + 0x0090);





            /////////////////////////////////////////////////////////////////////////////////////////////////////////////


            



            ///////////////////////////////////////////////// LOCATION ROTATION FOV ////////////////////////////////////////////////////////////////



            var rotator = new FRotator
            {
                Pitch = Pitch,
                Yaw = Yaw,
                Roll = Roll,
            };
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            /////////////////////////////////////////////// WORLD TO SCREEN ///////////////////////////////////////////////////////////////////////////////////////////

             
            ///////////////////////////////////////////////CAR TO BALL////////////////////////////////////////////////////////////////////////////////////////////////
            var bias_direction = (CarLocation - EnemyGoalLocation);
            bias_direction.Normalize();
            Vector3 target = BallLocation + bias_direction * 152;

            var aim = Math.Atan2(BallLocation.Y - CarLocation.Y, BallLocation.X - CarLocation.X);
            var aim2 = Math.Atan2(target.Y - CarLocation.Y, target.X - CarLocation.X);
            var cyawconvert = (CarYaw * (Math.PI / 32768.0));
            var front_to_target = aim - cyawconvert;
            var front_to_target2 = aim2 - cyawconvert;

            var CloseTo3 = Math.Abs(BallLocation.X - CarLocation.X);
            var CloseTo = Math.Abs(BallLocation.Y - CarLocation.Y);
            var CloseTo2 = Math.Atan2(BallLocation.X - CarLocation.X, BallLocation.Y - CarLocation.Y);
            var CloseTo4 = Math.Abs(CloseTo - CloseTo3);




            


            if (Math.Abs(front_to_target2) > Math.PI)
            {

                if (front_to_target2 < 0)
                    front_to_target2 += 2 * Math.PI;
                else
                    front_to_target2 -= 2 * Math.PI;

            }





            if (Math.Abs(front_to_target) > Math.PI)
            {

                if (front_to_target < 0)
                    front_to_target += 2 * Math.PI;
                else
                    front_to_target -= 2 * Math.PI;
               
            }


            


            if (Components.VisualsComponent.BallChase.Enabled)
            {

                
            if (front_to_target < 0)
                {
                    wasAPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.A, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasAPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.A, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasAPressed = false;
                    }
                    else
                    {

                    }
                }

                if (front_to_target > 0.16543621654)
                {
                    wasDPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.D, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasDPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.D, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasDPressed = false;
                    }
                    else
                    {

                    }
                }




                


                


                if (front_to_target > 2.2)
                {
                    wasLShiftPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasLShiftPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasLShiftPressed = false;
                    }
                    else
                    {

                    }
                };

                if (Components.VisualsComponent.AutoJump.Enabled)
                {
                    if (CloseTo4 <= 100)
                    {
                        wasRMousePressed = true;
                        Input.keybd_eventWS(VirtualKeyCode.RightMouse, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                    }
                    else
                    {
                        if (wasRMousePressed == true)
                        {
                            Input.keybd_eventWS(VirtualKeyCode.RightMouse, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                            wasRMousePressed = false;
                        }
                        else
                        {

                        }
                    };
                };

                if (Components.VisualsComponent.AimAtGoal.Enabled)
                {


                    
                    if (front_to_target2 < 0)
                    {
                        wasAPressed = true;
                        Input.keybd_eventWS(VirtualKeyCode.A, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                    }
                    else
                    {
                        if (wasAPressed == true)
                        {
                            Input.keybd_eventWS(VirtualKeyCode.A, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                            wasAPressed = false;
                        }
                        else
                        {

                        }
                    };
                }

                if (front_to_target2 > 0.060)
                {
                    wasDPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.D, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasDPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.D, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasDPressed = false;
                    }
                    else
                    {

                    }
                };

                

                if (front_to_target2 < -1.5)
                {
                    wasLShiftPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasLShiftPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasLShiftPressed = false;
                    }
                    else
                    {

                    }
                };

                if (front_to_target2 > 2)

                {
                    wasLShiftPressed = true;
                    Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                }
                else
                {
                    if (wasLShiftPressed == true)
                    {
                        Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasLShiftPressed = false;
                    }
                    else
                    {

                    }
                };

                if (Components.VisualsComponent.AutoJump.Enabled)
                {
                    if (CloseTo4 <= 100)
                    {
                        wasRMousePressed = true;
                        Input.keybd_eventWS(VirtualKeyCode.RightMouse, 0, KEYEVENTF.KEYDOWN, IntPtr.Zero);
                    }
                    else
                    {
                        if (wasRMousePressed == true)
                        {
                            Input.keybd_eventWS(VirtualKeyCode.RightMouse, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                            wasRMousePressed = false;
                        }
                        else
                        {

                        }

                    }
                }

            }
            else

            {
                if (wasAPressed == true) 
                {
                    Input.keybd_eventWS(VirtualKeyCode.A, 0, KEYEVENTF.KEYUP, IntPtr.Zero);

                    wasAPressed = false; 

                }
                else
                {
                    if (wasDPressed == true) 
                    {

                        Input.keybd_eventWS(VirtualKeyCode.D, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                        wasDPressed = false; 

                    }
                    else
                    {
                        if (wasLShiftPressed == true) 
                        {
                            Input.keybd_eventWS(VirtualKeyCode.LeftShift, 0, KEYEVENTF.KEYUP, IntPtr.Zero);

                            wasLShiftPressed = false; 

                        }
                        else
                        {
                            if (wasRMousePressed == true) 
                            {

                                Input.keybd_eventWS(VirtualKeyCode.RightMouse, 0, KEYEVENTF.KEYUP, IntPtr.Zero);
                                wasRMousePressed = false;

                            }
                            else
                            {

                            }
                        }
                    }
                }
            }

















            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////Ball To Goal Aimbot////////////////////////////////////////////////////////////

            Vector2 Pickups = new Vector2(0, 0);
            if (Renderer.WorldToScreenUE3(CarLocation, out Pickups, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
            {
                if (Components.VisualsComponent.DrawBallESP.Enabled)
                {

                    Renderer.DrawText(PickUp, Pickups, Color.Purple);
                }
            }



            Vector2 BallESP = new Vector2(0, 0);
            if (Renderer.WorldToScreenUE3(target, out BallESP, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
            {
                if (Components.VisualsComponent.DrawBallESP.Enabled)
                {

                    Renderer.DrawCircleFilled(BallESP, 6, Color.Red, 6);
                }
            }

            Vector2 GoalTeamESP = new Vector2(0, 0);
            if (Renderer.WorldToScreenUE3(TeamGoalLocation, out GoalTeamESP, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
                if (Components.VisualsComponent.TeamGoalESP.Enabled)
                {
                    
                    Renderer.DrawText("TEAM",GoalTeamESP, Color.Azure, 20);

                }

            Vector2 GoalEnemyESP = new Vector2(0, 0);
            if (Renderer.WorldToScreenUE3(EnemyGoalLocation, out GoalEnemyESP, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
                if (Components.VisualsComponent.EnemyGoalESP.Enabled)
                {
                    
                    Renderer.DrawText("ENEMY",GoalEnemyESP, Color.Azure, 20);

                }


            Vector2 Goal2 = new Vector2(0, 0);
            if (Renderer.WorldToScreenUE3(EnemyGoalLocation, out GoalEnemyESP, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
                if (Components.VisualsComponent.BallToGoalESP.Enabled)
                {
                    Renderer.DrawLine(BallESP, GoalEnemyESP, Color.Aquamarine, 2.654654654f);
                    
                }

            



            var Pills = new[] { Pill1, Pill2, Pill3, Pill4, Pill5, Pill6 };
            var BoostsArray = Memory.ReadPointer(processHandle, (IntPtr)(GameShare.ToInt64() + 0x0078), isWow64Process);
            var BoostsArrayCnt = Memory.ReadInt32(processHandle, (IntPtr)(GameShare.ToInt64() + 0x0080)); 
            for (int r = 0; r < BoostsArrayCnt; r++)
            {

                var currentBoost = Memory.ReadPointer(processHandle, (IntPtr)BoostsArray.ToInt64() + (r * 0x8), isWow64Process);

                var boostPos = Memory.ReadVector3(processHandle, (IntPtr)currentBoost.ToInt64() + 0x0090);


                string nameOrTime = "Pill" + (r + 1);

                if (!BoostsObjects.Contains(currentBoost.ToInt64()))
                    BoostsObjects.Add(currentBoost.ToInt64());



            }





            foreach (long objectPtr in BoostsObjects)
            {
                var isPicked = Memory.ReadBool(processHandle, (IntPtr)objectPtr + 0x02B8);

                if (isPicked)
                {
                    if (!BoostsTimers.ContainsKey(objectPtr))
                        BoostsTimers.Add(objectPtr, DateTime.Now.AddSeconds(10));
                }
            }

            foreach (var boostTimer in BoostsTimers.ToDictionary(x => x.Key, y => y.Value))
            {
                var boostPos = Memory.ReadVector3(processHandle, (IntPtr)boostTimer.Key + 0x0090);
                var timeLeft = (boostTimer.Value - DateTime.Now).TotalMilliseconds / 1000;
                var timeLeftStr = timeLeft.ToString("0.0");
                if (timeLeft <= 0)
                {
                    BoostsTimers.Remove(boostTimer.Key);
                    BoostsObjects.Remove(boostTimer.Key);
                    continue;
                }




                Vector2 PillVecOnScreen = new Vector2(0, 0);
                if (Renderer.WorldToScreenUE3(boostPos, out PillVecOnScreen, Location, rotator.Pitch, rotator.Yaw, rotator.Roll, LastCamFov, wndMargins, wndSize))
                    if (Components.VisualsComponent.DrawBoostTimer.Enabled)
                    {
                        Renderer.DrawText(timeLeftStr, PillVecOnScreen, Color.DarkOrange, 35, TextAlignment.centered, true);



                    }
                


            }



        }

        

    }

    
    public class FRotator
    {
        public int Pitch;
        public int Yaw;
        public int Roll;
    }

    



}
