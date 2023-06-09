using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace shoot_everything;

public class shoot_everything : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;
    private Vector aloituspaikka;

    private Image pelaajanKuva = LoadImage("shootter.png");
    private Image tahtiKuva = LoadImage("killfodder.png");

    private SoundEffect maaliAani = LoadSoundEffect("maali.wav");
    private int kenttanumero = 1;
    

    public override void Begin()
    
    {
        seuraavakentta();
        Gravity = new Vector(0, -1000);

        
        LisaaNappaimet();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;

        MasterVolume = 0.5;
    }

    void seuraavakentta()
    {
        ClearAll();
        if (kenttanumero == 1) LuoKentta("kentta1");
        else if (kenttanumero == 2) LuoKentta( "kentta2");
        
    }

    private void LuoKentta(string nimi)
    {
        TileMap kentta = TileMap.FromLevelAsset(nimi+".txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaVihollinen);
        kentta.SetTileMethod('N', LisaaPelaaja); 
        kentta.SetTileMethod('M',LisaaMaali);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }

    private void LisaaMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = PhysicsObject.CreateStaticObject(leveys, korkeus);
        maali.Position = paikka;
        maali.Color = Color.BloodRed;
        maali.Tag = "maali";
        Add(maali);

    }
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Green;
        Add(taso);
    }

    private void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject killfodder = new PhysicsObject(leveys, korkeus);
        killfodder.IgnoresCollisionResponse = false;
        killfodder.Position = paikka;
        killfodder.Image = tahtiKuva;
        killfodder.Tag = "killfodder";
        Add(killfodder); 
        
        
    }

    private void TormaaKillfodderiin(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        pelaaja1.Position = aloituspaikka;
    }
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "maali", Tormaamaaliin);
        AddCollisionHandler(pelaaja1, "killfodder", TormaaKillfodderiin);
        Add(pelaaja1);
        aloituspaikka = pelaaja1.Position;
        pelaaja1.Weapon = new AssaultRifle(30, 10);
        pelaaja1.Weapon.ProjectileCollision = AmmusOsui;
    }

    void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
    }
    void AmmuAseella(PlatformCharacter pelaaja)
    {
        PhysicsObject ammus = pelaaja.Weapon.Shoot();

        if (ammus != null)
        {
            //ammus.Size *= 3;
            //ammus.Image = ...
            //ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        }
    }

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        Keyboard.Listen(Key.Space, ButtonState.Pressed,AmmuAseella, "Pelaaja ampuu", pelaaja1);
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");

        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1,
            -NOPEUS);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    private void Tormaamaaliin(PhysicsObject hahmo, PhysicsObject maali)
    {
        maaliAani.Play();
        MessageDisplay.Add("bossihuone");
        kenttanumero++;
        Begin();

    }
}