# NOTES.md - Venise_AR_4

Date de mise a jour: 2026-03-20

## Etat actuel

- Le projet tourne sur Unity `6000.3.7f1` avec AR Foundation `6.3.3`.
- Vuforia a ete retire (packages/fichiers legacy nettoyes).
- Le flux navigation AR est actuellement:
  - `EntryScene` -> `ARScene` via `ButtonsManager.OnStartARExperienceClick()`
  - `ARScene` -> `EntryScene` via `ButtonsManager.OnBackToMainClick()`
- Dans `ButtonsManager`, `arSession.Reset()` est appele avant `LoadScene("EntryScene")`.
- `AR_UI_manager` contient un `return` defensif quand `content == null` (evite exception).
- Le raycast gameplay utilise un `LayerMask` dedie (`Raycast`) dans `Raycast.cs`.
- Les scripts `SphereController`, `RectangleController`, `ShowerController` utilisent `OnTriggerStay` (avec garde anti-retrigger) pour fiabiliser le declenchement des animations.
- Le probleme de deformation de `RectangularSculpture` (armature/weight paint non lisse) venait des `QualitySettings` mobiles trop bas.

## Etat script AR principal

Fichier: `Assets/Scripts/TargetHandler.cs`

- `args.added` declenche `TryPlaceAnchorAndContent(img)`.
- `args.updated` ajuste en continu la pose de l'ancre deja placee.
- Placement actuel volontairement non strict sur `TrackingState.Tracking` pour privilegier l'affichage rapide.
- `placed` sert de garde simple pour eviter de replacer un meme target key.

## Hypotheses Inspector importantes

- Dans `ARScene`, `TargetHandler` a bien ses references assignees:
  - `imageManager` -> composant `ARTrackedImageManager`
  - `anchorManager` -> composant `ARAnchorManager`
  - `sphereSculpture`, `rectangularSculpture`, `showerSculpture` -> prefabs/objets corrects
- Dans `ARScene`, `ButtonsManager.arSession` reference bien l'objet `AR Session`.

## Risques / points de vigilance

- Strategie actuelle (placement permissif puis correction en `updated`) peut produire un leger "snap" visuel initial.
- Si beaucoup d'evenements arrivent en rafale, garder un oeil sur une possible duplication rare d'ancre.
- Si Android/iOS repassent sur un profil qualite trop bas, le skinning peut redevenir degrade (weight paint non respecte).
- Le repo local peut etre dirty pendant iteration; verifier `git status` avant commit final.

## Procedure de verification rapide

1. Ouvrir `ARScene`.
2. Scanner une target: le content apparait.
3. Revenir a `EntryScene` via bouton retour.
4. Re-entrer dans `ARScene`.
5. Re-scanner: le content doit reapparaitre et suivre les updates sans erreur Console.
6. Interagir via raycast: seules les cibles du layer `Raycast` doivent reagir.
7. Verifier `RectangularSculpture`: deformation fluide (pas d'effet "rigide" des vertices).

## Prochaine etape

1. Verifier sur device Android/iOS la qualite active au runtime (quality level + skin weights).
2. Si besoin, forcer une qualite mobile AR explicite au demarrage (sans sur-correction).
3. Continuer la stabilisation incrementale de `TargetHandler` seulement si un symptome reel reapparait.
