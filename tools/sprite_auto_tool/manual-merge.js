const mergeBtn=$('mergeBtn');
let mergeTargets=[];
const baseRenderSource=renderSource;
const baseRenderFrames=renderFrames;
function cleanMergeTargets(){mergeTargets=mergeTargets.filter((v,i,a)=>v>=0&&v<frames.length&&a.indexOf(v)===i).slice(0,2)}
function updateMergeUI(){cleanMergeTargets();mergeBtn.disabled=mergeTargets.length!==2;mergeBtn.textContent=mergeTargets.length?`2つを結合 (${mergeTargets.length}/2)`:'2つを結合'}
function toggleMergeTarget(i){cleanMergeTargets();const p=mergeTargets.indexOf(i);if(p>=0)mergeTargets.splice(p,1);else if(mergeTargets.length<2)mergeTargets.push(i);else mergeTargets[1]=i;selected=i;renderSource();renderFrames();updateActionButtons();updateMergeUI();setStatus(mergeTargets.length===2?`#${mergeTargets[0]+1} と #${mergeTargets[1]+1} を選択しました。`:`結合対象 ${mergeTargets.length}/2。Ctrl/Cmd+クリックでもう1つ選択してください。`)}
renderSource=function(){baseRenderSource();if(!img)return;cleanMergeTargets();sctx.save();sctx.lineWidth=3;sctx.strokeStyle='#70e29b';sctx.fillStyle='#70e29b';sctx.font='bold 12px sans-serif';mergeTargets.forEach((i,n)=>{const r=frames[i];if(!r)return;sctx.strokeRect(r.x+1.5,r.y+1.5,Math.max(0,r.w-2),Math.max(0,r.h-2));sctx.fillText(`結合${n+1}`,r.x+4,r.y+29)});sctx.restore()};
renderFrames=function(){baseRenderFrames();cleanMergeTargets();[...els.list.children].forEach((card,i)=>card.classList.toggle('merge-target',mergeTargets.includes(i)));updateMergeUI()};
els.src.addEventListener('mousedown',e=>{if(!(e.ctrlKey||e.metaKey)||!img)return;const p=canvasPos(e),i=hitFrame(p.x,p.y);if(i<0)return;e.preventDefault();e.stopImmediatePropagation();toggleMergeTarget(i)},true);
els.list.addEventListener('click',e=>{if(!(e.ctrlKey||e.metaKey))return;const card=e.target.closest('.frame-card');if(!card)return;const i=[...els.list.children].indexOf(card);if(i<0)return;e.preventDefault();e.stopImmediatePropagation();toggleMergeTarget(i)},true);
mergeBtn.addEventListener('click',()=>{cleanMergeTargets();if(mergeTargets.length!==2)return;const [a,b]=mergeTargets.slice().sort((x,y)=>x-y);const merged=union(frames[a],frames[b]);frames.splice(b,1);frames.splice(a,1,merged);selected=a;mergeTargets=[];renderSource();renderFrames();updateActionButtons();drawPreview(selected);setStatus(`#${a+1} と #${b+1} を1つに結合しました。`)});
els.detect.addEventListener('click',()=>{mergeTargets=[];updateMergeUI()});
els.clear.addEventListener('click',()=>{mergeTargets=[];updateMergeUI()});
els.sort.addEventListener('click',()=>{mergeTargets=[];updateMergeUI()});
updateMergeUI();
