

## Codely Structured Memories

### User

### Feedback
- [2026-10-24 22:49:11] 2026-10-24: 用户遇到 bug 时先要「为什么」的解释，明确两次拒绝自动改动场景（"先只解释，不动场景" / "暂不修"）；给出根因+证据后再询问是否修改，不要直接改场景/脚本。
### Project
- [2026-10-24 22:49:18] 本项目是团结引擎(Tuanjie 2022.3.62t15) 2D 工程：场景用 .scene 扩展名（Assets/Scenes/SampleScene.scene，搜索 *.unity 会漏），脚本在 Assets/Scripts（Player.cs / FloorManager.cs / FloorMoveUp.cs），地板由 FloorManager 在运行时从 Assets/Prefabs 实例化成 (Clone)；TagManager 已注册 TianHuaBan/Floor1/Floor2/JianCi 以及拼错的 DaethField。
- [2026-10-25 00:26:12] 2026-10-25: 生成"奶蛙"受伤姿势（红温捧腹大笑）时 huoshan_seedream 后端必被 OutputImageSensitiveContentDetected.PolicyViolation 拒绝（疑似撞梗图版权）；改用 MCP generate_sprite_animation（frontier_flare，4×4 透明精灵表）可成功，再本地切帧（256×256）+ alpha>48 连通域去噪（保留1px软边）。全套动作帧在 Assets/Images/Player_{Idle,run,jump,hurt}/，命名 NaiWa_{Action}_{1..4}.png。
- [2026-10-31 20:15:29] 2026-10-26 跑步帧经验：sunburst 对"近侧/远侧腿"和"对侧摆臂"指令基本无视，永远画成"近侧手在胸前 + 近侧腿在前"；拿 3、4 当姿态参照要求"只换左右腿"时，产出与参照剪影 IoU 高达 0.77~0.86（等于近似重复帧，用户会判为顺拐）。可行做法：单帧分别生成 + 用 alpha>48 遮罩做 IoU/落地像素统计客观验收，别只靠肉眼；不行就换 provider（qwen/千问）。另：帧图 .meta 丢失会清空 run.anim 的精灵引用，需 AnimationUtility.SetObjectReferenceCurve 重绑；Player_run 里用户自放的 1.png/2.png 勿删。
- [2026-10-31 20:15:40] 本项目所有角色帧图（Assets/Images/Player_*）统一规格：256×256 透明 PNG、PPU=100、中心 pivot；处理帧时的归一化约定：角色包围盒高度缩放到 208px、脚底基线对齐 y=246、上半身质心对齐 x≈128（实测四帧质心 126~130、topY=38/bottomY=245 即合格）。

### Reference

