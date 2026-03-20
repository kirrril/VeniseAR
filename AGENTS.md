# AGENTS.md - Venise_AR_4

## Scope

Ces instructions s'appliquent a tout le projet `Venise_AR_4` (racine du repo).

## Demarrage de session

1. Lire `NOTES.md` avant toute proposition.
2. Reprendre depuis les sections `Etat actuel` et `Prochaine etape`.

## Contexte technique actuel

- Moteur: Unity `6000.3.7f1`
- Pipeline: URP (`com.unity.render-pipelines.universal` `17.3.0`)
- AR stack: AR Foundation only (`com.unity.xr.arfoundation` `6.3.3`)
- Providers: ARCore `6.3.3`, ARKit `6.3.3`
- Scenes build:
  - `Assets/Scenes/EntryScene.unity`
  - `Assets/Scenes/TargetScene.unity`
  - `Assets/Scenes/ARScene.unity`
- Scripts AR/UI cles:
  - `Assets/Scripts/TargetHandler.cs`
  - `Assets/Scripts/ButtonsManager.cs`
  - `Assets/Scripts/AR_UI_manager.cs`
  - `Assets/Scripts/Raycast.cs`
  - `Assets/Scripts/SphereController.cs`
  - `Assets/Scripts/RectangleController.cs`
  - `Assets/Scripts/ShowerController.cs`

## Regle de collaboration (obligatoire)

1. Proposer des solutions d'abord.
2. Ne jamais appliquer de changement (code, scene, ProjectSettings) sans demande explicite de l'utilisateur.
3. Si une modification est demandee, rester incremental et limiter le patch au strict necessaire.

## Priorites de travail

1. Stabilite du flux AR au retour dans `ARScene` (cycle added/updated).
2. Robustesse runtime (null checks, eviter exceptions en session).
3. Preserver l'existant qui fonctionne (pas de refactor large sans besoin).
4. Maintenir la compatibilite des scenes non AR (`EntryScene`, `TargetScene`).
5. Garantir une qualite mobile compatible skinned mesh (eviter profils trop bas pour les armatures).

## Regles de modification

1. Changements minimaux, lisibles, testables.
2. Eviter les changements de design systemique si un correctif local suffit.
3. Si un comportement depend de l'Inspector, expliciter l'hypothese.
4. Ne pas modifier les assets/scenes non concerns par la demande.

## Verification

1. Donner une procedure de test Unity courte apres chaque proposition.
2. Verifier au minimum:
   - premier lancement AR,
   - retour `EntryScene` -> `ARScene`,
   - detection target et affichage content,
   - absence d'erreur Console,
   - coherence visuelle des meshes skinnes (ex: `RectangularSculpture`),
   - filtrage raycast correct via layer dedie.

## Communication

- Repondre en francais, concis et actionnable.
- Donner les impacts et risques clairement avant tout changement.
