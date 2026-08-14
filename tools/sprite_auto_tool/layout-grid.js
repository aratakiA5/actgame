const layoutCols=$('layoutColumns'),insertBlankBtn=$('insertBlankBtn'),appendBlankBtn=$('appendBlankBtn'),removeBlankBtn=$('removeBlankBtn');
let layoutSlots=[],blankSelectedSlot=-1,draggedSlot=-1;
const priorRenderFrames=renderFrames;

function syncLayoutSlots(){
  const alive=new Set(frames);
  layoutSlots=layoutSlots.filter(x=>x===null||alive.has(x));
  const present=new Set(layoutSlots.filter(Boolean));
  frames.forEach(f=>{if(!present.has(f))layoutSlots.push(f)});
  if(!frames.length)layoutSlots=[];
  if(blankSelectedSlot>=layoutSlots.length)blankSelectedSlot=-1;
}
function resetLayoutToFrames(){layoutSlots=frames.slice();blankSelectedSlot=-1;renderFrames()}
function updateLayoutButtons(){
  const hasFrames=frames.length>0;
  insertBlankBtn.disabled=!hasFrames;
  appendBlankBtn.disabled=!hasFrames;
  removeBlankBtn.disabled=blankSelectedSlot<0||layoutSlots[blankSelectedSlot]!==null;
}
function slotForFrameIndex(i){const f=frames[i];return layoutSlots.indexOf(f)}
function frameIndexForSlot(i){const f=layoutSlots[i];return f?frames.indexOf(f):-1}

renderFrames=function(){
  syncLayoutSlots();
  priorRenderFrames();
  const cards=[...els.list.querySelectorAll('.frame-card')];
  cards.forEach((card,i)=>card.dataset.frameIndex=String(i));
  els.list.innerHTML='';
  els.list.style.setProperty('--layout-cols',Math.max(1,+layoutCols.value||8));
  layoutSlots.forEach((entry,slotIndex)=>{
    const slot=document.createElement('div');
    slot.className='layout-slot'+(entry===null?' blank':'');
    slot.dataset.slotIndex=String(slotIndex);
    if(entry===null){
      slot.textContent='空白';
      if(slotIndex===blankSelectedSlot)slot.classList.add('selected');
    }else{
      const fi=frames.indexOf(entry);
      const card=cards[fi];
      if(card){card.dataset.frameIndex=String(fi);slot.appendChild(card)}
    }
    els.list.appendChild(slot);
  });
  updateLayoutButtons();
};

layoutCols.addEventListener('input',()=>{
  const v=Math.max(1,+layoutCols.value||1);
  els.list.style.setProperty('--layout-cols',v);
  $('columns').value=v;
});
$('columns').addEventListener('input',()=>{if(document.activeElement===$('columns'))layoutCols.value=Math.max(1,+$('columns').value||1)});

insertBlankBtn.addEventListener('click',()=>{
  syncLayoutSlots();
  let at=layoutSlots.length;
  if(blankSelectedSlot>=0)at=blankSelectedSlot;
  else if(selected>=0){const s=slotForFrameIndex(selected);if(s>=0)at=s}
  layoutSlots.splice(at,0,null);blankSelectedSlot=at;renderFrames();setStatus(`位置 ${at+1} に空白セルを挿入しました。`)
});
appendBlankBtn.addEventListener('click',()=>{syncLayoutSlots();layoutSlots.push(null);blankSelectedSlot=layoutSlots.length-1;renderFrames();setStatus('末尾に空白セルを追加しました。')});
removeBlankBtn.addEventListener('click',()=>{if(blankSelectedSlot<0||layoutSlots[blankSelectedSlot]!==null)return;const n=blankSelectedSlot;layoutSlots.splice(n,1);blankSelectedSlot=-1;renderFrames();setStatus(`空白セル ${n+1} を削除しました。`)});

els.list.addEventListener('click',e=>{
  const slot=e.target.closest('.layout-slot');if(!slot)return;
  const si=+slot.dataset.slotIndex;
  if(layoutSlots[si]===null){e.preventDefault();e.stopImmediatePropagation();blankSelectedSlot=si;selected=-1;renderFrames();renderSource();updateActionButtons();setStatus(`空白セル ${si+1} を選択しました。`)}
  else blankSelectedSlot=-1;
},true);

els.list.addEventListener('dragstart',e=>{const slot=e.target.closest('.layout-slot');if(slot)draggedSlot=+slot.dataset.slotIndex},true);
els.list.addEventListener('dragover',e=>{const slot=e.target.closest('.layout-slot');if(!slot)return;e.preventDefault();slot.classList.add('drag-over')},true);
els.list.addEventListener('dragleave',e=>{const slot=e.target.closest('.layout-slot');if(slot)slot.classList.remove('drag-over')},true);
els.list.addEventListener('drop',e=>{
  const slot=e.target.closest('.layout-slot');if(!slot||draggedSlot<0)return;
  e.preventDefault();e.stopImmediatePropagation();
  const to=+slot.dataset.slotIndex;[layoutSlots[draggedSlot],layoutSlots[to]]=[layoutSlots[to],layoutSlots[draggedSlot]];
  blankSelectedSlot=layoutSlots[to]===null?to:-1;draggedSlot=-1;renderFrames();setStatus('フレーム配置を入れ替えました。');
},true);

els.detect.addEventListener('click',()=>setTimeout(resetLayoutToFrames,0));
els.clear.addEventListener('click',()=>{layoutSlots=[];blankSelectedSlot=-1;updateLayoutButtons()});
els.sort.addEventListener('click',()=>setTimeout(resetLayoutToFrames,0));

els.exportSheet.addEventListener('click',e=>{
  e.preventDefault();e.stopImmediatePropagation();
  syncLayoutSlots();if(!frames.length)return;
  const m=metrics(),sc=+$('exportScale').value,cols=Math.max(1,+layoutCols.value||1),rows=Math.ceil(layoutSlots.length/cols),out=document.createElement('canvas');
  out.width=Math.max(1,Math.round(m.w*cols*sc));out.height=Math.max(1,Math.round(m.h*rows*sc));
  const ctx=out.getContext('2d');ctx.imageSmoothingEnabled=false;
  layoutSlots.forEach((entry,i)=>{if(!entry)return;const fi=frames.indexOf(entry);if(fi<0)return;const n=normalizedFrame(fi,m),x=(i%cols)*m.w*sc,y=Math.floor(i/cols)*m.h*sc;ctx.drawImage(n,x,y,m.w*sc,m.h*sc)});
  downloadCanvas(out,'sprite_sheet.png');setStatus(`出力: ${out.width}×${out.height}px / ${frames.length}フレーム + 空白 ${layoutSlots.filter(x=>x===null).length} / ${cols}列`);
},true);

layoutCols.dispatchEvent(new Event('input'));
setTimeout(()=>{layoutSlots=frames.slice();renderFrames()},0);
