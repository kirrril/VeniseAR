# NOTES.md - Venise_AR_4

Date de mise a jour: 2026-03-27

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
- Le projet reference toujours `Mobile_RPAsset` comme pipeline URP global.
- Etat actuel de `Mobile_RPAsset` (`Assets/Settings/Mobile_RPAsset.asset`):
  - `HDR` desactive, `HDR Color Buffer Precision` sur valeur elevee.
  - `MSAA` a `4x`.
  - `Render Scale` a `0.6`.
  - `Main Light Shadowmap Resolution` a `2048`.
  - `Shadow Distance` a `40`.
  - `Shadow Cascade Count` a `2`.
  - `Soft Shadows` activees avec `Soft Shadow Quality` elevee.
- Etat actuel de la `Directional Light` de `ARScene`:
  - ombres en mode `Soft`.
  - `Intensity` a `0.5`.
  - `Shadow Strength` a `1.0`.
  - `UniversalAdditionalLightData` avec `Use Pipeline Settings` actif.
  - `Soft Shadow Quality` locale a `1`.
  - lumiere pilotee au runtime par `LightController`.
- Etat actuel de la `Directional Light` de `CuratorScene`:
  - ombres en mode `Soft`.
  - `Intensity` a `1.0`.
  - `Shadow Strength` a `0.77`.
  - `UniversalAdditionalLightData` avec `Use Pipeline Settings` desactive.
  - `Soft Shadow Quality` locale a `3`.
- Le `manual yaw` est pilote par un slider UI normalise (`0..1` -> `0..360`) via `OnValueChanged`.
- Un souci constate venait d'une reference `OnValueChanged` perdue dans l'Inspector (corrige).
- La persistance d'echelle des contenus AR est stabilisee:
  - `ScaleAdjustment.cs` est la source de verite pour lecture/ecriture `PlayerPrefs`.
  - `TargetHandler.cs` delegue l'application de l'echelle via `ApplyPersistedScale(content)` au placement.
  - Si la cle n'existe pas, elle est creee avec la valeur par defaut `0.5f`.
  - Le slider est synchronise sans boucle d'evenements via `SetValueWithoutNotify(...)`.
  - Une borne minimale effective `0.1f` est appliquee pour eviter un content invisible.
- Le placement AR de `TargetHandler` a ete ajuste pour les image targets posees a plat:
  - l'ancre est creee avec la position de `img.pose.position`,
  - la rotation n'utilise plus directement `img.pose.rotation`,
  - une rotation "upright" est reconstruite via `Vector3.up` (repere monde AR) + yaw projete depuis l'image trackee,
  - le content reste vertical tout en conservant le yaw de la target physique,
  - le snapping observe precedemment n'est plus reproduit dans ce mode de placement.
- `CuratorScene` est en place comme scene 3D non AR avec controle mobile par gizmo UI + gyro.
- Dans `CuratorScene`, `Main Camera` est enfant du `PlayerPrefab` (rattachee a `CameraPlace`) et porte `GyroCamera`.
- `MoveGizmo` est une `Image` UI (raycast target actif) dans un `Canvas` screen-space avec `GraphicRaycaster`.
- `EventSystem` de `CuratorScene` utilise `InputSystemUIInputModule`.
- Le flux de telechargement du PDF des image targets est en place via `PdfDownloadButton`:
  - source PDF attendue sur une URL publique HTTPS,
  - Android: telechargement systeme via `DownloadManager` vers `Downloads`,
  - iOS: telechargement local puis ouverture automatique de la share sheet native (`Imprimer` / `Enregistrer dans Fichiers`).
- `PlayerController` lit `MoveGizmo.speedX/speedY`:
  - `speedY` pour avance/recul,
  - `speedX` pour lateral + yaw fallback sans gyro.
- En mode gyro (`gyroIsOn`), `PlayerController.RotatePlayer()` aligne le yaw du player sur `Camera.main` via `MoveRotation`.
- Le petit snap de yaw au chargement de `CuratorScene` est toujours observable mais considere non bloquant a ce stade.

## Etat script AR principal

Fichier: `Assets/Scripts/TargetHandler.cs`

- `args.added` declenche `TryPlaceAnchorAndContent(img)`.
- `args.updated` ajuste en continu la pose de l'ancre deja placee via la meme logique "upright + yaw" que le placement initial.
- `GetUprightYawRotation(ARTrackedImage img)` projette un axe de l'image trackee sur le plan horizontal (`Vector3.up`) puis reconstruit une rotation debout avec `Quaternion.LookRotation(...)`.
- Le placement initial cree l'ancre avec `img.pose.position` et la rotation reconstruite, au lieu de recopier `img.pose.rotation`.
- Le comportement actuel est valide pour des image targets physiques posees a plat (content vertical + yaw de l'image).
- `placed` sert de garde simple pour eviter de replacer un meme target key.
- L'echelle n'est plus geree dans `TargetHandler` avec une logique locale `PlayerPrefs`; le script s'appuie sur `ScaleAdjustment`.

## Etat scripts CuratorScene

Fichiers:
- `Assets/Scripts/PlayerController.cs`
- `Assets/Scripts/MoveGizmo.cs`
- `Assets/Scripts/GyroCamera.cs`

Constats:
- `PlayerController`:
  - assigne position/rotation initiales depuis `entryPoint` au `Start()`.
  - ecrit `rb.linearVelocity` directement (avance/lateral selon valeurs gizmo).
  - en gyro: yaw du player force a celui de `Camera.main` (pas de lissage dedie).
  - sans gyro: yaw via `rb.angularVelocity` base sur `moveGizmo.speedX`.
- `MoveGizmo`:
  - derive `speedX/speedY` a partir du drag ecran (`Math.Clamp(..., -1, 1)`).
  - utilise `maxDragDistance = 200f`.
  - reset a zero sur `OnEndDrag`.
- `GyroCamera`:
  - active `AttitudeSensor` si disponible, fallback pose fixe sinon.
  - applique chaque frame `GyroToUnity(attitude)` sur la rotation de la camera.
  - expose `gyroAvailable` (etat lu par `PlayerController` au `Start()`).

Structure scene:
- `CuratorScene` ne contient plus de `CinemachineBrain` ni de `Virtual Camera`.
- Le child `CameraTarget` du `PlayerPrefab` est desactive dans l'instance de scene.
- Le prefab `PlayerPrefab` conserve un `PlayerInput` avec des `ActionEvents` historiques (`OnMove`, `OnLook`) alors que `PlayerController` ne depend plus de ces callbacks pour la locomotion actuelle.

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

- Si beaucoup d'evenements arrivent en rafale, garder un oeil sur une possible duplication rare d'ancre.
- La reconstruction de yaw depend de l'axe retenu (`img.transform.right`, fallback `img.transform.forward`); si l'orientation visuelle d'une target change, un offset fixe ou un autre axe pourra etre necessaire.
- Le comportement "upright" actuel est adapte aux image targets posees a plat; si des targets murales sont introduites plus tard, il faudra revalider la strategie de rotation.
- Si Android/iOS repassent sur un profil qualite trop bas, le skinning peut redevenir degrade (weight paint non respecte).
- La direction lumiere manuelle depend du bon wiring UI (`OnValueChanged`) si pilotage par slider.
- Sur iOS (ARKit world tracking), la direction de lumiere estimee peut etre indisponible selon device/contexte; garder un fallback manuel.
- Probleme visuel en cours: sur Samsung A26, les ombres restent visuellement tres `hard` en runtime dans `ARScene` malgre `Soft Shadows` activees dans `Mobile_RPAsset` et sur la `Directional Light`.
- Le rendu plus doux observe dans la vue `Scene` / Inspector n'est donc pas encore reproduit fidelement sur device Samsung A26.
- Le repo local peut etre dirty pendant iteration; verifier `git status` avant commit final.
- Le slider reste en plage `0..1`; avec la borne minimale effective `0.1`, la zone `0..0.1` produit la meme echelle visible (comportement volontaire).
- Le raycast suppose `hit.collider.transform.parent` comme racine content; si la hierarchie prefab change, la selection UI peut cibler le mauvais objet.
- `closeBurgerButton` doit etre cache au repos (ou enfant de `burgerContent`) pour eviter un etat UI ambigu lors d'une nouvelle detection.
- Disponibilite et stabilite du gyroscope peuvent varier selon device (absence capteur, bruit, drift); conserver un comportement stable sans capteur.
- `CuratorScene`: leger snap yaw au chargement (initialisation gyro) encore present.
- `CuratorScene`: `gyroIsOn` est evalue une seule fois au `Start()` dans `PlayerController` (pas de re-evaluation apres pause/reprise).
- `MoveGizmo`: reset uniquement sur `OnEndDrag`; selon contexte UI multitouch/interruptions, une remise a zero defensive supplementaire peut etre utile.
- `PdfDownloadButton`: la share sheet iOS n'est validable qu'en build device reel (pas dans l'Editor Unity).

## Procedure de verification rapide

1. Ouvrir `ARScene`.
2. Scanner une target: le content apparait.
3. Revenir a `EntryScene` via bouton retour.
4. Re-entrer dans `ARScene`.
5. Re-scanner: le content doit reapparaitre et suivre les updates sans erreur Console.
6. Verifier qu'un content associe a une target posee a plat reste vertical tout en conservant le yaw de la target.
7. Verifier l'absence de snapping visible au premier placement et pendant les `updated`.
8. Interagir via raycast: seules les cibles du layer `Raycast` doivent reagir.
9. Verifier `RectangularSculpture`: deformation fluide (pas d'effet "rigide" des vertices).
10. Verifier la persistance d'echelle:
   - premier scan d'un content: valeur par defaut `0.5`,
   - changement target A -> B -> A: la valeur de A est conservee,
   - slider au minimum: le content reste visible (echelle >= `0.1`).
11. Verifier le flux UI refactorise:
   - detection content: burger visible, panneau details ferme par defaut,
   - clic burger: cartel + sliders visibles,
   - perte de content au raycast: menu masque,
   - nouveau content: burger de nouveau cliquable avec infos du nouvel objet.
12. Verifier `EntryScene` (mode gyro decoratif):
   - sur device compatible: la camera suit l'attitude (`AttitudeSensor`),
   - sans capteur (ou capteur indisponible): fallback immobile sur vue initiale,
   - pas d'erreur Console a l'entree/sortie de scene (Enable/Disable).
13. Verifier `CuratorScene`:
   - drag `MoveGizmo` vertical -> avance/recul,
   - drag `MoveGizmo` horizontal -> lateral + yaw fallback (sans gyro),
   - mode gyro actif -> yaw player aligne sur camera sans mouvement parasite,
   - relachement drag -> vitesse gizmo revient a zero,
   - pas d'erreur Console.
14. Verifier le telechargement des image targets:
   - Android: clic bouton -> telechargement visible dans `Downloads`,
   - iPhone: clic bouton -> share sheet native ouverte apres telechargement,
   - options `Imprimer` et/ou `Enregistrer dans Fichiers` disponibles selon device/contexte,
   - pas d'erreur Console/Xcode pendant le flux.

## Prochaine etape

1. Verifier sur device Android/iOS la qualite active au runtime (quality level + skin weights).
2. Valider l'eclairage sur iOS et confirmer le comportement du fallback manuel de direction.
3. Sur Samsung A26, determiner si les hard shadows runtime viennent du `quality level` mobile actif, d'une limite URP mobile/device, ou d'un ecart entre `ARScene` et `CuratorScene`.
4. Si besoin, forcer une qualite mobile AR explicite au demarrage (sans sur-correction).
5. Comparer sur device Samsung A26 le rendu de la `Directional Light` de `ARScene` (pipeline settings) et celle de `CuratorScene` (override local) avant toute retouche plus large.
6. Continuer la stabilisation incrementale de `TargetHandler` seulement si un symptome reel reapparait.
7. Consolider la passe UI actuelle (verif device + Inspector) avant toute retouche ergonomique/visuelle.
8. Valider le ressenti gyro sur Android et iOS (fluidite, amplitude, confort visuel).
9. Ajouter un lissage leger anti-jitter dans `GyroCamera` seulement si un symptome reel apparait.
10. Decider si le snap yaw initial de `CuratorScene` merite un correctif (warmup/calibration) ou reste accepte.
11. Nettoyer `PlayerController` (`using` inutiles, null checks references Inspector) quand la phase de stabilisation est terminee.
12. Verifier si les `ActionEvents` historiques du `PlayerInput` (OnMove/OnLook) doivent etre conserves ou supprimes pour eviter ambiguite.
13. Valider sur iPhone reel le flux `PdfDownloadButton` (share sheet, impression, sauvegarde Fichiers).
