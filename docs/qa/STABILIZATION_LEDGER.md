# Stabilization ledger (RC preparation)

Authored corrections reviewed during this batch:

- Accepted-hit metrics bind run epochs, preserving committed hits across character changes while preventing old callbacks contaminating a new run.
- Pool creation stays inactive until lease ownership exists; cancellation removes owner bookkeeping before callbacks. VFX expiry and impact callbacks verify captured owner generations.
- Build verification rejects empty paths, unrelated artifact roots, malformed scene lists and modified artifact digests. Build wrapper requires a clean source checkout and matching commit.
- Test results remain timestamped; release gate requires XML provenance/digests, matching build GUID for device captures and explicit human QA/release acceptance.

Validation performed: seven Python verifier/provenance regressions passed; shell syntax and Python compilation passed. Static source reviews do not replace Unity import/compile, runtime soak or device tests. Those remain NOT RUN because the Unity Editor is unavailable in this environment. Complete the QA matrix, fix actual findings and retain regression evidence before closing stabilization. Ground-horde pooling/reset is authored and awaiting Unity lifecycle tests; it is not a validated reuse path.
