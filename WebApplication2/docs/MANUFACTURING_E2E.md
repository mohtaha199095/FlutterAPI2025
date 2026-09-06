# Manufacturing Module — End-to-End Test Checklist

Use this checklist after deploying DB version **10.61+** (phantom BOM, routing template, hourly rate, MO variance close).

## Prerequisites

1. Configure **Account Settings** → Manufacturing order Input / Output / Variance GL accounts.
2. Grant user authorization on forms **113–125** (BOM, MO, reports, work centers, scheduling, MRP, dashboard).
3. Ensure at least one **Branch**, **Store**, **Item** (raw + finished), and **UOM** exist.

## Test flow

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Settings → Account Settings → set MO Input / Output / Variance GL accounts | Accounts saved |
| 2 | Manufacturing → BOM → create BOM with inputs (+ Phantom if subassembly) + outputs (Cost % = 100) + Routing tab → Save | BOM saved via `SaveBOMFull` |
| 3 | Manufacturing → MO → Add MO → select BOM | Inputs explode (multi-level/phantom); planned qty includes Scrap %; routing copied |
| 4 | Save MO as Draft | MO saved; stay on screen with GUID assigned |
| 5 | Set status **Released** with insufficient stock | Save blocked with shortage message |
| 6 | Receive stock / lower planned qty → set **Released** → Save | MO released |
| 7 | Vouchers tab → Issue all materials (type 25) first | Voucher posts at **AVG cost**; MO → **In Progress** |
| 8 | Progress / Receive FG (type 26) | Blocked until inputs fully issued; if remaining, app prompts issue-first; unit cost from pool × CostShare % |
| 9 | Set status **Completed** → Save | Variance JV posted; WIP closed. Cannot reopen/cancel with posted vouchers; cannot delete vouchers after complete |
| 10 | MRP Suggestions | Shows MO + Purchase rows using min stock, sales offers, open MOs, explosion |
| 11 | Production Scheduling | Overloaded = Yes when planned hours exceed capacity × days |
| 12 | Work Centers → Hourly Rate | Labor absorbed into variance close when ActualHours > 0 |

## Auth verification

- MO list (116): view/search only unless delete granted.
- MO add/edit (120): create/edit MO, routing save, complete with variance.
- BOM add (114): create/edit BOM + routing; list delete uses form 113.

## API endpoints (smoke)

- `GET /api/ctlBOM/SelectBOMHeader`
- `GET /api/ctlBOM/ExplodeBOM`
- `GET /api/ctlBOM/SelectBOMRoutingByBOMID`
- `POST /api/ctlMO/SaveMOFull`
- `GET /api/ctlMO/SelectMOProgress`
- `GET /api/ctlMO/SelectReceiptCostAllocation`
- `GET /api/ctlMO/SelectMaterialAvailability`
- `POST /api/ctlMO/SaveMORoutingFull`
- `GET /api/ctlMO/CopyBOMRoutingToMO`
- `GET /api/ctlWorkCenter/SelectWorkCenter`
- `GET /api/ctlMO/SelectMRPSuggestions`
- `GET /api/ctlMO/SelectMODashboardSummary`
- `GET /api/ctlMO/SelectMODashboardOverdue`
