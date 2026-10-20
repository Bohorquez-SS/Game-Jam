import json, subprocess, os, numpy as np, soundfile as sf, librosa
from scipy.signal import butter, lfilter, fftconvolve
P='/tmp/piper/piper/piper'
M={'male':'/tmp/voice-es-carlfm-x-low/es-carlfm-x-low.onnx','female':'/tmp/voice-es-mls_10246-low/es-mls_10246-low.onnx'}
SR=22050
rng=np.random.default_rng(1)
def fnv(s):
    h=0x811C9DC5
    b=s.encode('utf-16-le')
    for i in range(0,len(b),2):
        h^= b[i] | (b[i+1]<<8); h=(h*0x01000193)&0xFFFFFFFF
    return '%08x'%h
def tts1(text, model, ls, ns, nw):
    tmp='/tmp/_t.wav'
    subprocess.run([P,'-m',M[model],'-f',tmp,'--length_scale',str(ls),'--sentence_silence','0.15','--noise_scale',str(ns),'--noise_w',str(nw)],input=text.encode('utf-8'),stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,check=True)
    x,sr=sf.read(tmp); return librosa.resample(x,orig_sr=sr,target_sr=SR)
def tts(text, model, ls):
    # varias tomas; se queda con la más corta que no sea absurdamente corta (evita "balbuceos" del modelo)
    lo=len(text)/20.0
    best=None
    params=[(0.3,0.3),(0.45,0.4),(0.3,0.2),(0.6,0.5)] if model=='female' else [(0.667,0.8),(0.4,0.5)]
    for ns,nw in params:
        x=tts1(text,model,ls,ns,nw); y,_=librosa.effects.trim(x,top_db=38); d=len(y)/SR
        if d>=lo and (best is None or d<best[0]): best=(d,x)
    return best[1] if best else x
def trim(x):
    y,_=librosa.effects.trim(x,top_db=38); return np.concatenate([np.zeros(int(0.05*SR)),y,np.zeros(int(0.12*SR))])
def bp(x,lo,hi): b,a=butter(2,[lo/(SR/2),hi/(SR/2)],'band'); return lfilter(b,a,x)
def lp(x,f): b,a=butter(2,f/(SR/2)); return lfilter(b,a,x)
def hp(x,f): b,a=butter(2,f/(SR/2),'high'); return lfilter(b,a,x)
def room(x,t=0.35,mix=0.18):
    n=int(t*SR); ir=rng.standard_normal(n)*np.exp(-np.arange(n)/SR*7/t); ir=lp(ir,3500); ir/=np.sqrt(np.sum(ir**2))
    y=fftconvolve(x,ir)[:len(x)+n]; out=np.zeros(len(y)); out[:len(x)]+=x; return out*(1-mix)+y*mix
def ps(x,st): return librosa.effects.pitch_shift(x,sr=SR,n_steps=st) if st else x
def norm(x,peak=0.8): m=np.max(np.abs(x)) or 1; return x/m*peak
def process(text, who):
    if who in ('prot','double'):
        x=trim(tts(text,'male',1.0)); x=ps(x,1.5); x=hp(x,90)
        if who=='double': x=room(x,0.5,0.25); x=lp(x,6000)
        return norm(x,0.8)
    if who=='dad':
        x=trim(tts(text,'male',1.12)); x=ps(x,-2.0); x=bp(x,280,3300)
        t=np.arange(len(x))/SR; x=x*(1+0.04*np.sin(2*np.pi*0.8*t))
        x=np.tanh(x*2.2)*0.5 + rng.standard_normal(len(x))*0.012
        return norm(room(x,0.2,0.1),0.7)
    if who=='guardian':
        x=trim(tts(text,'male',1.05)); x=ps(x,-1.0)
        t=np.arange(len(x))/SR; rm=x*np.sin(2*np.pi*55*t)
        y=x*0.75+rm*0.25; y=bp(y,180,5200)
        d=int(0.012*SR); y=y+np.concatenate([np.zeros(d),y[:-d]])*0.35
        q=np.round(y*64)/64; y=y*0.7+q*0.3
        return norm(room(y,0.6,0.22),0.75)
    if who in ('mom','fakemom'):
        x=trim(tts(text,'female',0.55)); x=hp(x,110)
        return norm(room(x,0.3,0.12),0.78)
    raise Exception(who)
def fake_level(text):
    if 'ABRE LA PUERTA' in text: return 3
    if 'te estoy diciendo' in text or 'Gracias por abrirme' in text: return 2
    if 'Por qué no me abres' in text or 'Eso es, mijo' in text: return 1
    return 0
def fakemom(text):
    lvl=fake_level(text)
    x=process(text,'mom')
    if lvl>=1: x=ps(x,-0.8*lvl)
    if lvl>=2:
        low=ps(x,-7.0)*0.45 if lvl==3 else ps(x,-5)*0.25
        n=min(len(x),len(low)); x=x[:n]+low[:n]
        x=np.tanh(x*(1.8 if lvl==2 else 3.2))
        t=np.arange(len(x))/SR; x=x*(1+0.25*np.sin(2*np.pi*(7 if lvl==2 else 13)*t))
    if lvl==3: x=room(x,0.9,0.35)
    return norm(x,0.8)
lines=json.load(open('lines.json',encoding='utf-8'))
os.makedirs('out',exist_ok=True)
index=[]
for text,who in lines.items():
    h=fnv(text)
    y=fakemom(text) if who=='fakemom' else process(text,who)
    sf.write('out/%s.wav'%h, y.astype(np.float32), SR, subtype='PCM_16')
    index.append((h,who,text,round(len(y)/SR,2)))
json.dump(index,open('index.json','w',encoding='utf-8'),ensure_ascii=False,indent=0)
print('done',len(index), round(sum(i[3] for i in index),1),'s')
