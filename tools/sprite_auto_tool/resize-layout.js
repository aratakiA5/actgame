(()=>{
  const workspace=document.getElementById('workspace');
  const v=document.getElementById('splitterV');
  const h=document.getElementById('splitterH');
  if(!workspace||!v||!h)return;

  const savedLeft=localStorage.getItem('spriteTool.leftPane');
  const savedPreview=localStorage.getItem('spriteTool.previewHeight');
  if(savedLeft)workspace.style.setProperty('--left-pane',savedLeft+'px');
  if(savedPreview)workspace.style.setProperty('--preview-height',savedPreview+'px');

  let mode=null;
  function start(which,e){
    mode=which;
    document.body.classList.add('resizing');
    (which==='v'?v:h).classList.add('dragging');
    e.preventDefault();
  }
  v.addEventListener('mousedown',e=>start('v',e));
  h.addEventListener('mousedown',e=>start('h',e));

  window.addEventListener('mousemove',e=>{
    if(!mode)return;
    const r=workspace.getBoundingClientRect();
    if(mode==='v'){
      const minLeft=440,minRight=520,gutter=14;
      const width=Math.max(minLeft,Math.min(e.clientX-r.left,r.width-minRight-gutter));
      workspace.style.setProperty('--left-pane',Math.round(width)+'px');
    }else{
      const minTop=260,minBottom=260,gutter=8;
      const preview=Math.max(minBottom,Math.min(r.bottom-e.clientY,r.height-minTop-gutter));
      workspace.style.setProperty('--preview-height',Math.round(preview)+'px');
    }
  });

  window.addEventListener('mouseup',()=>{
    if(!mode)return;
    const cs=getComputedStyle(workspace);
    const left=parseFloat(cs.getPropertyValue('--left-pane'));
    const preview=parseFloat(cs.getPropertyValue('--preview-height'));
    if(Number.isFinite(left))localStorage.setItem('spriteTool.leftPane',String(Math.round(left)));
    if(Number.isFinite(preview))localStorage.setItem('spriteTool.previewHeight',String(Math.round(preview)));
    document.body.classList.remove('resizing');
    v.classList.remove('dragging');
    h.classList.remove('dragging');
    mode=null;
    window.dispatchEvent(new Event('resize'));
  });
})();
