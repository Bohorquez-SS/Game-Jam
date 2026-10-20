# Escritorio de la PC del protagonista (original, inspirado en los escritorios azules de los 2000; sin logos reales).
import numpy as np, math
from PIL import Image, ImageDraw, ImageFilter, ImageFont
W,H=1024,768; S=2; w,h=W*S,H*S
F="/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"; FB="/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"; FM="/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf"
y,x=np.mgrid[0:h,0:w].astype(np.float32)
# fondo: degradado azul con un resplandor central claro
cx,cy=w*0.52,h*0.42
d=np.sqrt(((x-cx)/w)**2+((y-cy)/h*0.8)**2)
base=np.dstack([0.02+0.0*x,0.28+0.0*x,0.62+0.0*x])
glow=np.clip(1-d*1.6,0,1)**1.6
img=base*(1-glow[...,None])+np.array([0.35,0.75,1.0])*glow[...,None]
dark=np.clip((y/h-0.55)*1.2,0,1)[...,None]*np.clip(((x/w)-0.3),0,1)[...,None]
img=img*(1-0.45*dark)
# rayos de luz desde abajo a la izquierda
ang=np.arctan2(y-h*1.05,x-w*0.12)
rays=(np.sin(ang*38)*0.5+0.5)**8*np.clip(1-np.sqrt(((x-w*0.12)/w)**2+((y-h*1.05)/h)**2)*1.1,0,1)
img+=rays[...,None]*np.array([0.25,0.35,0.4])*0.6
im=Image.fromarray((np.clip(img,0,1)*255).astype(np.uint8))
dr=ImageDraw.Draw(im,'RGBA')
# líneas curvas finas (estelas)
for k,(a,b,c) in enumerate([(0.05,0.92,0.18),(0.0,0.84,0.12),(0.1,0.97,0.22)]):
    pts=[(w*(t/60),h*(b-c*math.sin(t/60*math.pi*0.9)+0.02*math.sin(t/7))) for t in range(61)]
    dr.line(pts,fill=(170,255,140,140 if k==0 else 90),width=3*S)
# bokeh
rng=np.random.default_rng(3)
for i in range(26):
    r=rng.uniform(8,40)*S; px=rng.uniform(0.05,0.6)*w; py=rng.uniform(0.35,0.95)*h
    dr.ellipse([px-r,py-r,px+r,py+r],fill=(255,255,255,int(rng.uniform(10,35))))
im=im.filter(ImageFilter.GaussianBlur(1.2*S)); dr=ImageDraw.Draw(im,'RGBA')
# emblema central original: luna creciente + estrella
mx,my,R=w*0.53,h*0.40,120*S
halo=Image.new('RGBA',(w,h)); hd=ImageDraw.Draw(halo)
hd.ellipse([mx-R*1.6,my-R*1.6,mx+R*1.6,my+R*1.6],fill=(255,255,255,70)); halo=halo.filter(ImageFilter.GaussianBlur(40*S)); im.paste(halo,(0,0),halo)
dr=ImageDraw.Draw(im,'RGBA')
moon=Image.new('L',(w,h),0); md=ImageDraw.Draw(moon)
md.ellipse([mx-R,my-R,mx+R,my+R],fill=255); md.ellipse([mx-R*0.45,my-R*1.15,mx+R*1.4,my+R*0.75],fill=0)
gold=Image.new('RGBA',(w,h),(255,214,120,255)); im.paste(gold,(0,0),moon)
dr=ImageDraw.Draw(im,'RGBA')
def star(cx,cy,r,col):
    pts=[]
    for i in range(10):
        a=-math.pi/2+i*math.pi/5; rr=r if i%2==0 else r*0.42
        pts.append((cx+rr*math.cos(a),cy+rr*math.sin(a)))
    dr.polygon(pts,fill=col)
star(mx+R*0.55,my-R*0.35,26*S,(255,240,190,255)); star(mx+R*0.95,my+R*0.25,12*S,(255,240,190,220))
f=ImageFont.truetype(FB,30*S); t="NocheOS"; tw=dr.textlength(t,font=f); dr.text((mx-tw/2,my+R+18*S),t,font=f,fill=(255,255,255,235))
# iconos del escritorio
fi=ImageFont.truetype(F,13*S)
def icon(px,py,kind,label):
    if kind=='pc':
        dr.rounded_rectangle([px+8*S,py+4*S,px+56*S,py+38*S],4*S,fill=(210,215,225,255)); dr.rectangle([px+12*S,py+8*S,px+52*S,py+33*S],fill=(40,120,210,255))
        dr.rectangle([px+26*S,py+38*S,px+38*S,py+46*S],fill=(180,185,195,255))
    elif kind=='bin':
        dr.polygon([(px+16*S,py+10*S),(px+48*S,py+10*S),(px+44*S,py+48*S),(px+20*S,py+48*S)],fill=(200,225,240,230)); dr.rectangle([px+13*S,py+5*S,px+51*S,py+10*S],fill=(170,200,220,255))
    elif kind=='folder':
        dr.rounded_rectangle([px+8*S,py+12*S,px+56*S,py+46*S],3*S,fill=(240,200,90,255)); dr.rectangle([px+8*S,py+8*S,px+28*S,py+14*S],fill=(230,185,70,255))
    elif kind=='doc':
        dr.rectangle([px+16*S,py+4*S,px+48*S,py+48*S],fill=(250,250,250,255))
        for k in range(5): dr.line([px+21*S,py+(12+k*7)*S,px+43*S,py+(12+k*7)*S],fill=(120,140,170,255),width=2*S)
    elif kind=='cam':
        dr.rounded_rectangle([px+10*S,py+14*S,px+54*S,py+42*S],5*S,fill=(60,65,75,255)); dr.ellipse([px+24*S,py+18*S,px+42*S,py+36*S],fill=(30,30,35,255),outline=(150,200,255,255),width=2*S)
        dr.ellipse([px+46*S,py+17*S,px+51*S,py+22*S],fill=(230,40,40,255))
    tw=dr.textlength(label,font=fi)
    dr.text((px+32*S-tw/2+1*S,py+52*S+1*S),label,font=fi,fill=(0,0,0,180)); dr.text((px+32*S-tw/2,py+52*S),label,font=fi,fill=(255,255,255,255))
for i,(k,l) in enumerate([('pc','Mi PC'),('bin','Papelera'),('folder','Tareas'),('doc','notas.txt'),('cam','GUARDIAN')]):
    icon(18*S,(18+i*86)*S,k,l)
# ventana de notas (historia)
wx,wy,ww,wh=640*S,70*S,350*S,250*S
sh=Image.new('RGBA',(w,h)); sd=ImageDraw.Draw(sh); sd.rounded_rectangle([wx+6*S,wy+8*S,wx+ww+6*S,wy+wh+8*S],8*S,fill=(0,0,0,110)); sh=sh.filter(ImageFilter.GaussianBlur(8*S)); im.paste(sh,(0,0),sh)
dr=ImageDraw.Draw(im,'RGBA')
dr.rounded_rectangle([wx,wy,wx+ww,wy+wh],8*S,fill=(150,190,225,235),outline=(40,70,110,255),width=S)
dr.text((wx+12*S,wy+7*S),"notas.txt - Bloc de notas",font=ImageFont.truetype(F,13*S),fill=(20,30,50,255))
for i,c in enumerate([(120,170,215),(120,170,215),(200,60,50)]):
    bx=wx+ww-(3-i)*30*S-6*S; dr.rounded_rectangle([bx,wy+5*S,bx+26*S,wy+21*S],3*S,fill=c+(255,))
dr.text((wx+ww-24*S,wy+3*S),"x",font=ImageFont.truetype(FB,14*S),fill=(255,255,255,255))
dr.rectangle([wx+6*S,wy+28*S,wx+ww-6*S,wy+wh-6*S],fill=(255,255,255,255))
fm=ImageFont.truetype(FM,15*S)
for i,line in enumerate(["sábado 23:47","- terminar la tarea de física","- lluvia otra vez","- la cámara del pasillo hace","  ruido (decirle a mamá)","- NO apagar la alarma","","_"]):
    dr.text((wx+14*S,wy+(36+i*24)*S),line,font=fm,fill=(20,20,30,255))
# barra de tareas
th=46*S
bar=Image.new('RGBA',(w,th)); bd=ImageDraw.Draw(bar)
for i in range(th):
    t=i/th; c=(int(60-30*t),int(110-50*t),int(150-60*t),215); bd.line([(0,i),(w,i)],fill=c)
bd.line([(0,0),(w,0)],fill=(200,230,255,200),width=S)
im.paste(bar,(0,h-th),bar); dr=ImageDraw.Draw(im,'RGBA')
# botón de inicio redondo con la luna
ox,oy,orr=30*S,h-th/2,19*S
dr.ellipse([ox-orr,oy-orr,ox+orr,oy+orr],fill=(40,120,60,255),outline=(190,240,200,255),width=2*S)
dr.ellipse([ox-orr*0.8,oy-orr*0.9,ox+orr*0.8,oy-orr*0.05],fill=(255,255,255,70))
dr.ellipse([ox-8*S,oy-8*S,ox+8*S,oy+8*S],fill=(255,220,130,255)); dr.ellipse([ox-4*S,oy-11*S,ox+11*S,oy+5*S],fill=(40,120,60,255))
# botones fijados
for i,col in enumerate([(240,200,90),(70,150,230),(230,90,60)]):
    bx=70*S+i*62*S
    dr.rounded_rectangle([bx,h-th+5*S,bx+54*S,h-5*S],5*S,fill=(255,255,255,40 if i else 70),outline=(255,255,255,60))
    dr.rounded_rectangle([bx+15*S,h-th+12*S,bx+39*S,h-12*S],4*S,fill=col+(255,))
# bandeja y reloj
fc=ImageFont.truetype(F,14*S)
for t,yy in (("23:47",h-th+6*S),("sáb 17/10",h-th+24*S)):
    tw=dr.textlength(t,font=fc); dr.text((w-16*S-tw,yy),t,font=fc,fill=(255,255,255,255))
for i in range(3): dr.rectangle([w-(140+i*22)*S,h-th+17*S,w-(128+i*22)*S,h-th+29*S],fill=(220,230,240,200))
im=im.convert('RGB').resize((W,H),Image.LANCZOS)
im.save('screen_desktop.png'); print('ok')
