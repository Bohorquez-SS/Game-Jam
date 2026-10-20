import re, json, glob
def parse_args(s, i):
    # s[i] == '(' ; return list of top-level args, end index
    depth=0; args=[]; cur=''; j=i; instr=False
    while j < len(s):
        c=s[j]
        if instr:
            cur+=c
            if c=='\\': cur+=s[j+1]; j+=2; continue
            if c=='"': instr=False
        elif c=='"': instr=True; cur+=c
        elif c=='(':
            depth+=1
            if depth>1: cur+=c
        elif c==')':
            depth-=1
            if depth==0: args.append(cur.strip()); return args, j
            cur+=c
        elif c==',' and depth==1: args.append(cur.strip()); cur=''
        else: cur+=c
        j+=1
    return args, j
def first_lit(e):
    m=re.search(r'"((?:[^"\\]|\\.)*)"', e)
    return None if not m else m.group(1).encode('utf-8').decode('unicode_escape').encode('latin-1').decode('utf-8') if False else (m.group(1).replace('\\"','"').replace('\\n','\n') if m else None)
def classify(sp):
    if sp in ('You',) or '"Tú"' in sp: return 'prot'
    if 'voz igual a la tuya' in sp or 'Tu propia voz' in sp: return 'double'
    if '(afuera)' in sp and '¿Mamá?' in sp: return 'fakemom'
    if sp.startswith('T("¿Mamá?"'): return 'fakemom'
    if 'Mamá' in sp: return 'mom'
    if sp=='dad' or 'Papá' in sp: return 'dad'
    if 'GUARDIÁN' in sp: return 'guardian'
    return None
lines={}
files=glob.glob('Scripts/Story/*.cs')+['Scripts/Systems/GameManager.cs','Scripts/Stations/BreakerStation.cs']
for f in files:
    s=open(f,encoding='utf-8').read()
    for m in re.finditer(r'\.Say\(', s):
        args,_=parse_args(s, m.end()-1)
        if len(args)<2: continue
        who=classify(args[0])
        if not who: continue
        txt=first_lit(args[1]) if args[1].startswith('T(') or args[1].startswith('Settings.T(') or args[1].startswith('"') else None
        if txt: lines[txt]=who
    # arrays
    for m in re.finditer(r'string\[\] es\s*=\s*\{(.*?)\};', s, re.S):
        who={'StoryC4.cs':'dad','StoryC5.cs':'double','StoryC3.cs':'double'}.get(f.split('/')[-1])
        if not who: continue
        for lit in re.findall(r'"((?:[^"\\]|\\.)*)"', m.group(1)):
            lines[lit.replace('\\"','"').replace('\\n','\n')]=who
json.dump(lines, open('voices/lines.json','w',encoding='utf-8'), ensure_ascii=False, indent=1)
from collections import Counter
print(len(lines), Counter(lines.values()))
