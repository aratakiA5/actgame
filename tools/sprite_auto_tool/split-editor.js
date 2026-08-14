(()=>{
  const splitBtn=document.getElementById('splitEditBtn');
  if(!splitBtn)return;

  const root=document.createElement('div');
  root.className='split-editor';
  root.innerHTML=`
    <div class="split-editor-bar">
      <h2>分割編集</h2>
      <label>分割方向
        <select id="splitDirection"><option value="tb">上 / 下</option><option value="lr">左 / 右</option></select>
      </label>
      <label>補完幅 <input id="splitOverlap" type="number" min="0" max="64" value="4"> px</label>
      <label>表示倍率 <input id="splitZoom" type="number" min="1" max="12" step="1" value="4"> 倍</label>
      <button id="splitClear" type="button">線を消す</button>
      <div class="spacer"></div>
      <button id="splitCancel" type="button">キャンセル</button>
      <button id="splitApply" type="button" class="primary" disabled>分割確定</button>
    </div>
    <div class="split-editor-main">
      <div id="splitWork" class="split-work"><canvas id="splitCanvas"></canvas></div>
      <aside class="split-side">
        <div class="split-preview-card"><h3>A プレビュー</h3><div class="split-preview-wrap"><canvas id="splitPreviewA"></canvas></div></div>
        <div class="split-preview-card"><h3>B プレビュー</h3><div class="split-preview-wrap"><canvas id="splitPreviewB"></canvas></div></div>
        <div class="split-help">画像上をドラッグして分割線を描きます。上/下モードでは線より上がA、下がBです。補完幅の範囲は両方に残るため、境界付近の髪・手・武器などを欠けにくくできます。</div>
      </aside>
    </div>`;
  document.body.appendChild(root);

  const work=$('splitCanvas'),wctx=work.getContext('2d');
  const prevA=$('splitPreviewA'),prevB=$('splitPreviewB');
  const dir=$('splitDirection'),overlap=$('splitOverlap'),zoom=$('splitZoom');
  const clearBtn=$('splitClear'),cancelBtn=$('splitCancel'),applyBtn=$('splitApply');
  let editingIndex=-1,baseFrame=null,source=null,points=[],drawing=false,lastSplit=null;

  const originalFrameCrop=frameCrop;
  frameCrop=function(f){
    if(f&&f._splitCanvas)return f._splitCanvas;
    return originalFrameCrop(f);
  };

  const originalUpdateActionButtons=updateActionButtons;
  updateActionButtons=function(){
    originalUpdateActionButtons();
    splitBtn.disabled=selected<0||selected>=frames.length;
  };

  function cloneCanvas(c){
    const out=document.createElement('canvas');out.width=c.width;out.height=c.height;
    const ctx=out.getContext('2d');ctx.imageSmoothingEnabled=false;ctx.drawImage(c,0,0);return out;
  }
  function openEditor(){
    if(selected<0||selected>=frames.length)return;
    stopAnim?.();
    editingIndex=selected;baseFrame=frames[selected];source=cloneCanvas(frameCrop(baseFrame));points=[];lastSplit=null;
    root.classList.add('open');document.body.style.overflow='hidden';applyBtn.disabled=true;drawWork();drawPreviews(null);
  }
  function closeEditor(){root.classList.remove('open');document.body.style.overflow='';drawing=false;points=[];lastSplit=null}
  splitBtn.addEventListener('click',openEditor);
  cancelBtn.addEventListener('click',closeEditor);
  clearBtn.addEventListener('click',()=>{points=[];lastSplit=null;applyBtn.disabled=true;drawWork();drawPreviews(null)});
  zoom.addEventListener('input',drawWork);dir.addEventListener('change',()=>{if(points.length>1)refreshSplit();else drawWork()});
  overlap.addEventListener('input',()=>{if(points.length>1)refreshSplit()});

  function drawWork(){
    if(!source)return;const z=Math.max(1,+zoom.value||1);
    work.width=source.width*z;work.height=source.height*z;wctx.imageSmoothingEnabled=false;
    wctx.clearRect(0,0,work.width,work.height);wctx.drawImage(source,0,0,work.width,work.height);
    if(points.length){wctx.save();wctx.strokeStyle='#ff4d67';wctx.lineWidth=Math.max(2,z*.7);wctx.lineJoin='round';wctx.lineCap='round';wctx.beginPath();wctx.moveTo(points[0].x*z,points[0].y*z);for(let i=1;i<points.length;i++)wctx.lineTo(points[i].x*z,points[i].y*z);wctx.stroke();wctx.restore()}
  }
  function pointerPos(e){const r=work.getBoundingClientRect(),z=Math.max(1,+zoom.value||1);return{x:Math.max(0,Math.min(source.width-1,(e.clientX-r.left)*work.width/r.width/z)),y:Math.max(0,Math.min(source.height-1,(e.clientY-r.top)*work.height/r.height/z))}}
  work.addEventListener('pointerdown',e=>{if(!source)return;drawing=true;points=[pointerPos(e)];work.setPointerCapture?.(e.pointerId);drawWork()});
  work.addEventListener('pointermove',e=>{if(!drawing)return;const p=pointerPos(e),q=points[points.length-1];if(!q||Math.hypot(p.x-q.x,p.y-q.y)>=.5){points.push(p);drawWork()}});
  work.addEventListener('pointerup',e=>{if(!drawing)return;drawing=false;const p=pointerPos(e);points.push(p);refreshSplit()});

  function buildBoundary(axisLength,raw,keyMain,keyCross){
    const pts=raw.map(p=>({m:p[keyMain],c:p[keyCross]})).sort((a,b)=>a.m-b.m);
    const compact=[];for(const p of pts){const last=compact[compact.length-1];if(last&&Math.abs(last.m-p.m)<.4){last.c=(last.c+p.c)/2;last.m=(last.m+p.m)/2}else compact.push({...p})}
    if(compact.length<2)return null;
    const out=new Float32Array(axisLength);let seg=0;
    for(let i=0;i<axisLength;i++){
      while(seg<compact.length-2&&i>compact[seg+1].m)seg++;
      if(i<=compact[0].m)out[i]=compact[0].c;
      else if(i>=compact[compact.length-1].m)out[i]=compact[compact.length-1].c;
      else{const a=compact[seg],b=compact[seg+1],t=(i-a.m)/Math.max(.0001,b.m-a.m);out[i]=a.c+(b.c-a.c)*t}
    }
    return out;
  }
  function splitCanvas(){
    if(!source||points.length<2)return null;const sw=source.width,sh=source.height,ov=Math.max(0,+overlap.value||0),mode=dir.value;
    const srcCtx=source.getContext('2d',{willReadFrequently:true}),src=srcCtx.getImageData(0,0,sw,sh),aData=new ImageData(sw,sh),bData=new ImageData(sw,sh);
    const boundary=mode==='tb'?buildBoundary(sw,points,'x','y'):buildBoundary(sh,points,'y','x');if(!boundary)return null;
    for(let y=0;y<sh;y++)for(let x=0;x<sw;x++){
      const si=(y*sw+x)*4,alpha=src.data[si+3];if(!alpha)continue;
      const d=mode==='tb'?y-boundary[x]:x-boundary[y];
      if(d<=ov){aData.data[si]=src.data[si];aData.data[si+1]=src.data[si+1];aData.data[si+2]=src.data[si+2];aData.data[si+3]=alpha}
      if(d>=-ov){bData.data[si]=src.data[si];bData.data[si+1]=src.data[si+1];bData.data[si+2]=src.data[si+2];bData.data[si+3]=alpha}
    }
    const a=document.createElement('canvas'),b=document.createElement('canvas');a.width=b.width=sw;a.height=b.height=sh;a.getContext('2d').putImageData(aData,0,0);b.getContext('2d').putImageData(bData,0,0);return{a,b};
  }
  function alphaBounds(c){
    const ctx=c.getContext('2d',{willReadFrequently:true}),d=ctx.getImageData(0,0,c.width,c.height).data;let minx=c.width,miny=c.height,maxx=-1,maxy=-1;
    for(let y=0;y<c.height;y++)for(let x=0;x<c.width;x++)if(d[(y*c.width+x)*4+3]){if(x<minx)minx=x;if(x>maxx)maxx=x;if(y<miny)miny=y;if(y>maxy)maxy=y}
    return maxx<0?null:{x:minx,y:miny,w:maxx-minx+1,h:maxy-miny+1};
  }
  function cropMasked(c,b){const out=document.createElement('canvas');out.width=b.w;out.height=b.h;const ctx=out.getContext('2d');ctx.imageSmoothingEnabled=false;ctx.drawImage(c,b.x,b.y,b.w,b.h,0,0,b.w,b.h);return out}
  function refreshSplit(){lastSplit=splitCanvas();applyBtn.disabled=!lastSplit;drawWork();drawPreviews(lastSplit)}
  function drawPreviewCanvas(target,c){const ctx=target.getContext('2d');if(!c){target.width=1;target.height=1;ctx.clearRect(0,0,1,1);return}target.width=c.width;target.height=c.height;ctx.imageSmoothingEnabled=false;ctx.clearRect(0,0,target.width,target.height);ctx.drawImage(c,0,0)}
  function drawPreviews(result){drawPreviewCanvas(prevA,result?.a||null);drawPreviewCanvas(prevB,result?.b||null)}

  applyBtn.addEventListener('click',()=>{
    const result=lastSplit||splitCanvas();if(!result||editingIndex<0||editingIndex>=frames.length)return;
    const ba=alphaBounds(result.a),bb=alphaBounds(result.b);if(!ba||!bb){setStatus('分割結果の片方が空です。分割線を調整してください。');return}
    const old=frames[editingIndex],ca=cropMasked(result.a,ba),cb=cropMasked(result.b,bb);
    const fa={x:old.x+ba.x,y:old.y+ba.y,w:ba.w,h:ba.h,_splitCanvas:ca};
    const fb={x:old.x+bb.x,y:old.y+bb.y,w:bb.w,h:bb.h,_splitCanvas:cb};
    let slot=-1;if(typeof layoutSlots!=='undefined'&&Array.isArray(layoutSlots))slot=layoutSlots.indexOf(old);
    frames.splice(editingIndex,1,fa,fb);
    if(slot>=0){layoutSlots.splice(slot,1,fa,fb)}
    selected=editingIndex;mergeTargets=[];closeEditor();renderSource();renderFrames();updateActionButtons();drawPreview(selected);
    setStatus(`フレーム #${editingIndex+1} を A/B の2フレームへ分割しました。補完幅 ${Math.max(0,+overlap.value||0)}px。`);
  });

  document.addEventListener('keydown',e=>{if(!root.classList.contains('open'))return;if(e.key==='Escape')closeEditor()});
  updateActionButtons();
})();
