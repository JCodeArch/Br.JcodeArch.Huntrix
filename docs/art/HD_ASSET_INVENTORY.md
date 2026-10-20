# HD asset inventory — baseline 20/10/2026

Source snapshot: c8feb37, based on c42fbd0 plus preserved pre-existing changes. Original directory is unchanged. Full art manifest: outputs/hd-validation/baseline-20261020/art-files.csv.

| Group | Identities | Current consumption | Required recovery |
|---|---|---|---|
| Playable trio | Rumi, Mira, Zoey | Numeric 0..3; Slice uses Idle/Run/Jump/Attack | State clips, common foot anchors; actual Dash/Parry/Hurt/Death/Special/Aerial; attack phase timing |
| Rivals | Jinu, Abby, Mystery, Romance, Baby | Single Characters image; partially hash-selected | Explicit identity and animation sets; separate portrait mapping |
| Final boss | GwiMa | Static Characters image | Boss-specific pose/state clips |
| Fans | KpopFan | Single 512-square image tinted by real fan state | Explicit Normal/Drained/Critical/Corrupted mapping; avoid pretending six disconnected decorative states are integrated |
| Cabin enemies | CaptainDokkaebi, MagentaStewardess, BlueCyclopsCrew | Base plus 4 morphs; first two have 4 action frames | Distinct role identity, full telegraph/contact/recovery and boss behavior |
| Other demons | SmokeImp, CabinBrute, GumihoFox, CorruptedHaetae, BulgasariBrute, TalchumPhantom, WingedJeoseung, DokkaebiShield | Base image and procedural transform | Explicit identity, appropriate motion and hit reactions |

Parallel CHR_* frames and sheets are currently ignored by numeric loaders. No PNG LFS pointers found by specialist. Stages/airplane.png has JPEG data: validate/re-export without changing meta identity. Existing presentation normalizes using full canvas bounds; 512-square versus 384x512 frames can change apparent anatomy scale. Approved preview sheet is visual reference, not animation atlas.

No specific Celine/Bobby/Derpy/Sussie assets found in inspected source. This is an inventory gap to reconcile with GDD, not a new scope cancellation.

No runtime acceptance is claimed by this inventory.
