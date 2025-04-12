using System;
using BepInEx;
using On.HUD;
using On.Menu;
using RWCustom;
using UnityEngine;
using MenuLabel = Menu.MenuLabel;
using MenuObject = Menu.MenuObject;
using SimpleButton = Menu.SimpleButton;

namespace KarmaControl;

[BepInPlugin("risottoman.karmacontrol", "Karma Control", "1.3.0")]
public class Plugin : BaseUnityPlugin
{
    private bool _init;
    private RainWorldGame _game;
    private HUD.KarmaMeter _meter;
    private Menu.PauseMenu _menu;
    private MenuLabel _menuLabel;
    
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (!this._init)
            {
                On.RainWorldGame.ctor += RainWorldGameOnCtor;
                PauseMenu.ctor += PauseMenuOnCtor;
                PauseMenu.Singal += PauseMenuOnSingal;
                KarmaMeter.ctor += KarmaMeterOnCtor;
                this._init = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void KarmaMeterOnCtor(KarmaMeter.orig_ctor orig, HUD.KarmaMeter self, HUD.HUD hud, FContainer fContainer, IntVector2 displayKarma, bool showAsReinforced)
    {
        orig(self, hud, fContainer, displayKarma, showAsReinforced);
        this._meter = self;
    }

    private void RainWorldGameOnCtor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        this._game = self;
    }
    
    private void PauseMenuOnCtor(PauseMenu.orig_ctor orig, Menu.PauseMenu self, ProcessManager manager, RainWorldGame game)
    {
        orig(self, manager, game);
        this._menu = self;
        
        if (game.session is not StoryGameSession)
            return;
        
        float height = 180.2f;
        Vector2 offset = self.manager.rainWorld.options.SafeScreenOffset;
        Single xPos = self.ContinueAndExitButtonsXPos - height - offset.x;
        
        MenuLabel capLabel = new MenuLabel(self, self.pages[0], "MODIFY KARMA CAP", new Vector2(xPos + 7f, Mathf.Max(offset.y, 200f)), new Vector2(100f, 30f), false);
        self.pages[0].subObjects.Add(capLabel);
        
        MenuLabel karmaLabel = new MenuLabel(self, self.pages[0], "MODIFY KARMA", new Vector2(xPos + 5f, Mathf.Max(offset.y, 260f)), new Vector2(100f, 30f), false);
        self.pages[0].subObjects.Add(karmaLabel);
        
        SimpleButton button1 = new SimpleButton(self, self.pages[0], "+1", "ADDKARMA", new Vector2(xPos + 55f, Mathf.Max(offset.y, 230f)), new Vector2(55f, 30f));
        self.pages[0].subObjects.Add(button1);
        
        SimpleButton button2 = new SimpleButton(self, self.pages[0], "-1", "SUBKARMA", new Vector2(xPos, Mathf.Max(offset.y, 230f)), new Vector2(55f, 30f));
        self.pages[0].subObjects.Add(button2);
        
        SimpleButton button3 = new SimpleButton(self, self.pages[0], "+1", "ADDKARMACAP", new Vector2(xPos + 55f, Mathf.Max(offset.y, 170f)), new Vector2(55f, 30f));
        self.pages[0].subObjects.Add(button3);
        
        SimpleButton button4 = new SimpleButton(self, self.pages[0], "-1", "SUBKARMACAP", new Vector2(xPos, Mathf.Max(offset.y, 170f)), new Vector2(55f, 30f));
        self.pages[0].subObjects.Add(button4);
        
        var data = this._game.GetStorySession.saveState.deathPersistentSaveData;
        this._menuLabel = new MenuLabel(self, self.pages[0], $"KARMA: {data.karma + 1} / {data.karmaCap + 1}", new Vector2(xPos, Mathf.Max(offset.y, 140f)), new Vector2(100f, 30f), false);
        
        self.pages[0].subObjects.Add(this._menuLabel);
    }
    
    private void PauseMenuOnSingal(PauseMenu.orig_Singal orig, Menu.PauseMenu self, MenuObject sender, string message)
    {
        orig(self, sender, message);

        if (this._game.session is not StoryGameSession)
            return;
        
        var data = this._game.GetStorySession.saveState.deathPersistentSaveData;
        
        switch (message)
        {
            case "ADDKARMA":
            {
                if (data.karma < data.karmaCap)
                {
                    data.karma++;
                }
                break;
            }
            
            case "SUBKARMA":
            {
                if (data.karma > 0)
                {
                    data.karma--;
                }
                break;
            }

            case "ADDKARMACAP":
            {
                if (data.karmaCap < 9)
                {
                    data.karmaCap++;
                }
                break;
            }

            case "SUBKARMACAP":
            {
                if (data.karmaCap > 0)
                {
                    data.karmaCap--;
                    
                    if (data.karma >= data.karmaCap)
                    {
                        data.karma = data.karmaCap;
                    }
                }
                
                break;
            }
        }

        this._menuLabel.label.text = $"KARMA: {data.karma + 1} / {data.karmaCap + 1}";
        this._meter.UpdateGraphic();
    }
}