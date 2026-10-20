import numpy as np
from PIL import Image
N=1024
rng=np.random.default_rng(7)
def pnoise(cx, cy, seed):
    """Ruido periódico (tileable) filtrado en frecuencia: cx,cy = longitud de correlación en px por eje."""
    r=np.random.default_rng(seed)
    w=r.standard_normal((N,N))
    F=np.fft.fft2(w)
    fy=np.fft.fftfreq(N)[:,None]; fx=np.fft.fftfreq(N)[None,:]
    g=np.exp(-((fx*cx)**2+(fy*cy)**2)*2*np.pi**2)
    n=np.real(np.fft.ifft2(F*g)); n=(n-n.mean())/(n.std()+1e-9)
    return n
def lerp(a,b,t): return a+(b-a)*t
def wood(base_lo, base_hi, planks, seed, seams=True, ring_freq=55, warp=10.0, stagger=True):
    y,x=np.mgrid[0:N,0:N].astype(np.float32)
    pw=N/planks
    pid=np.floor(x/pw).astype(int)
    # desplazamiento vertical por tabla (vetas y juntas diferentes en cada tabla)
    out=np.zeros((N,N,3),np.float32); height=np.zeros((N,N),np.float32)
    low=pnoise(60,500,seed)            # deformación suave de las vetas, muy estirada en Y
    fib=pnoise(1.2,90,seed+1)          # fibras finas
    pores=pnoise(0.8,6,seed+2)         # poros
    blotch=pnoise(120,240,seed+3)
    for p in range(planks):
        m=(pid==p)
        r=np.random.default_rng(seed*31+p)
        off=r.uniform(0,N); tint=r.uniform(-0.08,0.08); freq=ring_freq*r.uniform(0.75,1.3)
        cx=r.uniform(-2,2)*pw
        xl=(x-p*pw-pw/2+cx)
        # anillos casi rectos: distancia al centro del tronco + deformación
        yy=((y+off)%N)/N*2*np.pi
        arch=(np.cos(yy*r.integers(1,3))*0.5+0.5)**3*pw*r.uniform(0.15,0.6)   # vetas en arco (catedral), periódicas
        d=np.sqrt((xl + low*warp*r.uniform(0.6,1.4))**2 + 1.0) + arch
        period=pw/freq
        ring=0.5+0.5*np.sin(d/period*2*np.pi + 0.8*np.roll(low,int(off),axis=0))
        ring=ring**3
        t=np.clip(0.36*ring+0.12*fib+0.10*blotch+0.42+tint,0,1)
        col=lerp(np.array(base_lo),np.array(base_hi),t[...,None])
        col*= (1-0.06*np.clip(pores,0,3)[...,None])
        out[m]=col[m]; height[m]=(0.6*ring+0.2*fib)[m]
    if seams:
        g=(np.abs((x % pw))<2)|(np.abs((x % pw)-pw)<2)
        out[g]*=0.45; height[g]=-1
        if stagger:
            for p in range(planks):
                r=np.random.default_rng(seed*77+p); jy=int(r.uniform(0,N))
                m=(pid==p)&(np.abs(y-jy)<2)
                out[m]*=0.5; height[m]=-1
    return np.clip(out,0,1), height
def normal(h, strength=2.0):
    gx=(np.roll(h,-1,1)-np.roll(h,1,1))*strength; gy=(np.roll(h,-1,0)-np.roll(h,1,0))*strength
    n=np.dstack([-gx,-gy,np.ones_like(h)]); n/=np.linalg.norm(n,axis=2,keepdims=True)
    return ((n*0.5+0.5)*255).astype(np.uint8)
def save(name,c,h,s=1.5):
    Image.fromarray((c*255).astype(np.uint8)).save(name+'.png')
    Image.fromarray(normal(h,s)).save(name+'_n.png')
c,h=wood([0.42,0.27,0.14],[0.68,0.49,0.30],3,11,seams=False,ring_freq=9,warp=22); save('wood_light',c,h)
c,h=wood([0.14,0.07,0.035],[0.34,0.20,0.10],3,21,seams=False,ring_freq=9,warp=22); save('wood_dark',c,h)
c,h=wood([0.32,0.17,0.085],[0.58,0.37,0.20],6,31,seams=True,ring_freq=5,warp=12); save('wood_floor',c,h,2.5)
