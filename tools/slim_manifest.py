"""Replace base64 data URIs in a fal manifest with sha256 stubs naming the tracked file they match, if any.

Run from the repo root after a fal batch: python3 tools/slim_manifest.py assets/gen/fal_manifest.jsonl assets/gen
"""
import base64, hashlib, json, os, sys
manifest, gen = sys.argv[1], sys.argv[2]
known = {}
for name in os.listdir(gen):
    if name.endswith('.png'):
        known[hashlib.sha256(open(os.path.join(gen, name), 'rb').read()).hexdigest()] = f'assets/gen/{name}'
def stub(v):
    if isinstance(v, str) and v.startswith('data:') and ';base64,' in v:
        data = v.split(';base64,', 1)[1]
        try:
            raw = base64.b64decode(data)
        except ValueError:
            return f'(data URI already truncated in the manifest: {len(data)} base64 chars)'
        h = hashlib.sha256(raw).hexdigest()
        return f'sha256:{h} ({len(raw)} bytes){" = " + known[h] if h in known else ""}'
    if isinstance(v, list):
        return [stub(x) for x in v]
    if isinstance(v, dict):
        return {k: stub(x) for k, x in v.items()}
    return v
out, matched, total = [], 0, 0
for line in open(manifest).read().splitlines():
    r = json.loads(line)
    if 'input' in r:
        r['input'] = stub(r['input'])
    s = json.dumps(r, ensure_ascii=False)
    total += s.count('sha256:'); matched += s.count(' = assets/gen/')
    out.append(s)
open(manifest, 'w').write('\n'.join(out) + '\n')
print(f'{len(out)} entries, {total} images stubbed, {matched} matched to tracked files, {os.path.getsize(manifest)} bytes')
