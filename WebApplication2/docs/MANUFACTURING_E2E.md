# Manufacturing Module — End-to-End Test Checklist

Use this checklist after deploying DB version **10.14+** (form page names) and **10.15+** (work centers, routing, MRP).

## Prerequisites

1. Configure **Account Settings** → Manufacturing order Input / Output / Variance GL accounts.
2. Grant user authorization on forms **113–125** (BOM, MO, reports, work centers, scheduling, MRP, dashboard).
3. Ensure at least one **Branch**, **Store**, **Item** (raw + finished), and **UOM** exist.

## Test flow

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Settings → Account Settings → set MO GL accounts | Accounts saved |
| 2 | Manufacturing → BOM → create BOM with inputs + outputs → Save | BOM saved via `SaveBOMFull` |
| 3 | Manufacturing → MO → Add MO → select BOM | Input/output lines scale with Planned × Batch / BOM batch |
| 4 | Save MO | MO saved; stay on screen with GUID assigned |
| 5 | Routing tab → add work center steps → Save Routing | Lines stored in `tbl_MORouting` |
| 6 | Set status **Released** → Vouchers tab → Issue (type 25) | `InvoicePageAdd` opens; voucher posts |
| 7 | Return to MO → voucher linked | Link appears in Vouchers tab |
| 8 | Progress tab | Actual input qty/cost updated |
| 9 | Receive finished goods (type 26) | Output progress updated |
| 10 | MO Summary / Progress / Vouchers reports | Data matches MO |
| 11 | Manufacturing Dashboard | KPI cards show counts |
| 12 | Production Scheduling | MO/routing rows visible for date range |
| 13 | MRP Suggestions | Shortfall items listed when stock below minimum |
| 14 | Accounting → verify JV | Types 25/26 posted like GI/GR |

## Auth verification

- MO list (116): view/search only unless delete granted.
- MO add/edit (120): create/edit MO, routing save.
- BOM add (114): create/edit BOM; list delete uses form 113.

## API endpoints (smoke)

- `GET /api/ctlBOM/SelectBOMHeader`
- `POST /api/ctlMO/SaveMOFull`
- `GET /api/ctlMO/SelectMOProgress`
- `POST /api/ctlMO/SaveMORoutingFull`
- `GET /api/ctlWorkCenter/SelectWorkCenter`
- `GET /api/ctlMO/SelectMRPSuggestions`
- `GET /api/ctlMO/SelectMODashboardSummary`
