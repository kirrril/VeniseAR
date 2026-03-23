# NOTES.md - Venise_AR_4

Date de mise a jour: 2026-03-23

## Etat actuel

- Le projet tourne sur Unity `6000.3.7f1` avec AR Foundation `6.3.3`.
- Vuforia a ete retire (packages/fichiers legacy nettoyes).
- Le flux navigation AR est actuellement:
  - `EntryScene` -> `ARScene` via `ButtonsManager.OnStartARExperienceClick()`
  - `ARScene` -> `EntryScene` via `ButtonsManager.OnBackToMainClick()`
- `EntryScene` utilise `GyroCamera` avec le nouveau Input System (`AttitudeSensor`) pour un decor/point de vue immersif pilote au gyroscope.
- Fallback valide dans `GyroCamera`: si le capteur n'est pas disponible, la camera reste immobile sur `initPosition` / `initRotation`.
- Renforcement runtime valide: `OnDisable()` protege `InputSystem.DisableDevice(...)` avec un null check sur `AttitudeSensor.current`.
- Dans `ButtonsManager`, `arSession.Reset()` est appele avant `LoadScene("EntryScene")`.
- `AR_UI_manager` contient un `return` defensif quand `content == null` (evite exception).
- Le flux UI "burger + cartel" est en place et coherent avec le raycast:
  - detection d'un content (`SelectionChanged != null`) -> affichage `contentMenu` + `burgerButton`.
  - clic burger -> affichage du cartel (titre/artiste/annee/dimensions) + sliders de correction.
  - perte de content (`SelectionChanged == null`) -> masquage du menu.
  - nouveau content detecte -> menu re-initialise et infos rechargees.
- Le raycast gameplay utilise un `LayerMask` dedie (`Raycast`) dans `Raycast.cs`.
- Les scripts `SphereController`, `RectangleController`, `ShowerController` utilisent `OnTriggerStay` (avec garde anti-retrigger) pour fiabiliser le declenchement des animations.
- Le probleme de deformation de `RectangularSculpture` (armature/weight paint non lisse) venait des `QualitySettings` mobiles trop bas.
- Un script `LightController` est en place pour piloter une `Directional Light` globale depuis `ARCameraManager.frameReceived`.
- Sur Android, l'estimation automatique de lumiere donne un resultat visuel satisfaisant en l'etat.
- Le `manual yaw` est pilote par un slider UI normalise (`0..1` -> `0..360`) via `OnValueChanged`.
- Un souci constate venait d'une reference `OnValueChanged` perdue dans l'Inspector (corrige).
- La persistance d'echelle des contenus AR est stabilisee:
  - `ScaleAdjustment.cs` est la source de verite pour lecture/ecriture `PlayerPrefs`.
  - `TargetHandler.cs` delegue l'application de l'echelle via `ApplyPersistedScale(content)` au placement.
  - Si la cle n'existe pas, elle est creee avec la valeur par defaut `0.5f`.
  - Le slider est synchronise sans boucle d'evenements via `SetValueWithoutNotify(...)`.
  - Une borne minimale effective `0.1f` est appliquee pour eviter un content invisible.

## Etat script AR principal

Fichier: `Assets/Scripts/TargetHandler.cs`

- `args.added` declenche `TryPlaceAnchorAndContent(img)`.
- `args.updated` ajuste en continu la pose de l'ancre deja placee.
- Placement actuel volontairement non strict sur `TrackingState.Tracking` pour privilegier l'affichage rapide.
- `placed` sert de garde simple pour eviter de replacer un meme target key.
- L'echelle n'est plus geree dans `TargetHandler` avec une logique locale `PlayerPrefs`; le script s'appuie sur `ScaleAdjustment`.

## Hypotheses Inspector importantes

- Dans `ARScene`, `TargetHandler` a bien ses references assignees:
  - `imageManager` -> composant `ARTrackedImageManager`
  - `anchorManager` -> composant `ARAnchorManager`
  - `sphereSculpture`, `rectangularSculpture`, `showerSculpture` -> prefabs/objets corrects
- Dans `ARScene`, `ButtonsManager.arSession` reference bien l'objet `AR Session`.
- Dans `ARScene`, `AR_UI_manager` a bien ses references assignees:
  - `raycast`, `contentMenu`, `burgerButton`, `burgerContent`, `closeBurgerButton`, `closeSceneButton`.
  - `title`, `artist`, `year`, `dimensions` (TMP_Text) relies aux champs UI.

## Risques / points de vigilance

- Strategie actuelle (placement permissif puis correction en `updated`) peut produire un leger "snap" visuel initial.
- Si beaucoup d'evenements arrivent en rafale, garder un oeil sur une possible duplication rare d'ancre.
- Si Android/iOS repassent sur un profil qualite trop bas, le skinning peut redevenir degrade (weight paint non respecte).
- La direction lumiere manuelle depend du bon wiring UI (`OnValueChanged`) si pilotage par slider.
- Sur iOS (ARKit world tracking), la direction de lumiere estimee peut etre indisponible selon device/contexte; garder un fallback manuel.
- Le repo local peut etre dirty pendant iteration; verifier `git status` avant commit final.
- Le slider reste en plage `0..1`; avec la borne minimale effective `0.1`, la zone `0..0.1` produit la meme echelle visible (comportement volontaire).
- Le raycast suppose `hit.collider.transform.parent` comme racine content; si la hierarchie prefab change, la selection UI peut cibler le mauvais objet.
- `closeBurgerButton` doit etre cache au repos (ou enfant de `burgerContent`) pour eviter un etat UI ambigu lors d'une nouvelle detection.
- Disponibilite et stabilite du gyroscope peuvent varier selon device (absence capteur, bruit, drift); conserver un comportement stable sans capteur.

## Procedure de verification rapide

1. Ouvrir `ARScene`.
2. Scanner une target: le content apparait.
3. Revenir a `EntryScene` via bouton retour.
4. Re-entrer dans `ARScene`.
5. Re-scanner: le content doit reapparaitre et suivre les updates sans erreur Console.
6. Interagir via raycast: seules les cibles du layer `Raycast` doivent reagir.
7. Verifier `RectangularSculpture`: deformation fluide (pas d'effet "rigide" des vertices).
8. Verifier la persistance d'echelle:
   - premier scan d'un content: valeur par defaut `0.5`,
   - changement target A -> B -> A: la valeur de A est conservee,
   - slider au minimum: le content reste visible (echelle >= `0.1`).
9. Verifier le flux UI refactorise:
   - detection content: burger visible, panneau details ferme par defaut,
   - clic burger: cartel + sliders visibles,
   - perte de content au raycast: menu masque,
   - nouveau content: burger de nouveau cliquable avec infos du nouvel objet.
10. Verifier `EntryScene` (mode gyro decoratif):
   - sur device compatible: la camera suit l'attitude (`AttitudeSensor`),
   - sans capteur (ou capteur indisponible): fallback immobile sur vue initiale,
   - pas d'erreur Console a l'entree/sortie de scene (Enable/Disable).

## Prochaine etape

1. Verifier sur device Android/iOS la qualite active au runtime (quality level + skin weights).
2. Valider l'eclairage sur iOS et confirmer le comportement du fallback manuel de direction.
3. Si besoin, forcer une qualite mobile AR explicite au demarrage (sans sur-correction).
4. Continuer la stabilisation incrementale de `TargetHandler` seulement si un symptome reel reapparait.
5. Consolider la passe UI actuelle (verif device + Inspector) avant toute retouche ergonomique/visuelle.
6. Valider le ressenti gyro sur Android et iOS (fluidite, amplitude, confort visuel).
7. Ajouter un lissage leger anti-jitter dans `GyroCamera` seulement si un symptome reel apparait.
