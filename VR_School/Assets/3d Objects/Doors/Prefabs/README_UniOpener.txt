README - UniOpener Ansteuerung

Prefab benutzen, nicht das .fbx!

Dieses Script steuert Türen, Fenster, Schiebetüren, Kippfenster und ähnliche Objekte.

--------------------------------------------------
ANSTEUERUNG
--------------------------------------------------

Normales Öffnen / Schließen:

    uniOpener.ToggleOpen();

Normales Kippen / Schliessen (optional, wenn aktiviert)

    uniOpener.ToggleTilt();



ToggleOpen() schaltet den normalen Öffnungszustand um:

    geschlossen -> offen
    offen -> geschlossen

Beispiel:

    using UnityEngine;

    public class ExampleInteraction : MonoBehaviour
    {
        [SerializeField] private UniOpener uniOpener;

        public void Interact()
        {
            uniOpener.ToggleOpen();
        }
    }

Gezielt öffnen:

    uniOpener.Open();

Gezielt schließen:

    uniOpener.Close();

Kippen / zurückstellen:

    uniOpener.ToggleTilt();

ToggleTilt() schaltet den Kippzustand um:

    geschlossen -> gekippt
    gekippt -> geschlossen

Gezielt kippen:

    uniOpener.Tilt();

Gezielt Kippstellung schließen:

    uniOpener.CloseTilt();

Wichtig:

Open und Tilt sind alternativ.

    Open aktiv  -> Tilt wird ignoriert
    Tilt aktiv  -> Open wird ignoriert

ToggleOpen() darf Open wieder schließen.
ToggleTilt() darf Tilt wieder schließen.

--------------------------------------------------
INSPECTOR
--------------------------------------------------

References:

Audio Source
    AudioSource für Open Sound und Close Sound.

Object To Animate
    Hauptobjekt, das bewegt oder rotiert wird.
    Wenn leer, wird automatisch das Objekt mit dem Script benutzt.

Object To Animate 2
    Optionales zweites Objekt für Schiebetüren.
    Wird nur bei Sliding benutzt.
    Bewegt sich gegenläufig zu Object To Animate.

Handle To Animate
    Optionaler Griff / Klinke / Fenstergriff.

State/Test:

Is Open
    Test-Häkchen für normales Öffnen.

Is Tilted
    Test-Häkchen für Kippen.

Rotation Animation:

Closed Angle
    Winkel im geschlossenen Zustand.

Open Angle
    Winkel im geöffneten Zustand.

Anim Speed
    Geschwindigkeit der Rotation.

Local Rotation Axis
    Lokale Achse für normales Öffnen.

Tilting Animation:

Enable Tilting
    Erlaubt Kippen.
    Bei Türen deaktivieren.

Tilt Closed Angle
    Winkel im geschlossenen Zustand für Tilt.

Tilt Open Angle
    Winkel im gekippten Zustand.

Tilt Anim Speed
    Geschwindigkeit der Kippbewegung.

Local Tilt Rotation Axis
    Lokale Achse für Kippen.

Tilt Handle Settings:

Tilt Handle Angle
    Griffwinkel beim Kippen.

Tilt Handle Rotation Axis
    Lokale Achse für den Griff beim Kippen.

Sliding Animation:

Enable Sliding
    Aktiviert Schiebebewegung statt Rotation.

Slide Distance
    Strecke der Schiebebewegung.

Slide Speed
    Geschwindigkeit der Schiebebewegung.

Local Slide Axis
    Lokale Achse der Schiebebewegung.

Handle Animation:

Enable Handle Animation
    Aktiviert Griffanimation.

Keep Handle Position
    Griff bleibt in Zielstellung, bis wieder geschlossen wird.

Handle Down Angle
    Griffwinkel beim normalen Öffnen.

Handle Animation Time
    Dauer der Griffanimation.

Handle Rotation Axis
    Lokale Achse für normale Griffbewegung.

Audiofiles:

Open Sound
    Sound beim Öffnen.
    Startet sofort, auch während der Griffanimation.

Loop Open Sound
    Open Sound wird während der Öffnungsanimation geloopt.

Close Sound
    Sound beim Schließen.
    Wird kurz vor Ende der Schließbewegung abgespielt.

Loop Close Sound
    Close Sound wird ab seinem Startzeitpunkt geloopt.

Volume
    Lautstärke für beide Sounds.

Close Sound Timing:

Close Sound Delay
    Bestimmt, wie kurz vor Ende der Schließbewegung der Close Sound startet.
