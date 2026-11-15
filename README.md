# Lcc

[Lcc](https://github.com/404Lcc/Lcc) 是一个 Unity 游戏开发框架，包含模块化基础服务、资源与代码热更新、UI 与本地化、网络与 Steam 房间接入，以及分层战斗框架。仓库同时包含通用框架和当前项目的游戏业务，复用时可从框架层与业务入口开始阅读。

## 开发环境

- Unity **2023.2.20f1**，以 [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) 为准。
- URP **16.0.6**、Input System **1.7.0**、YooAsset **2.3.14**。
- HybridCLR、Luban、LitMotion、NavMeshPlus 通过 Git 包引用，完整依赖见 [Packages/manifest.json](Packages/manifest.json)。首次打开需要能够访问对应 Git 仓库与 OpenUPM 源。
- 数据表导出工具使用 **.NET 8** 运行时；仅运行 Unity 项目时不需要执行导表脚本。

当前仓库按完整 Unity 工程打开。根目录 `package.json` 仍保留旧版 Unity / ILRuntime 声明，不能作为当前工程的安装与兼容性依据。

## 主要功能

| 模块 | 当前能力 |
| --- | --- |
| 模块与流程 | 通过 `Main` / `Module` 组织服务和生命周期，提供游戏流程、协程、计时器和主线程同步 |
| 资源与热更新 | 基于 YooAsset 的资源加载、版本与清单更新、资源下载、缓存清理；基于 HybridCLR 的热更程序集加载与 AOT 元数据补充 |
| UI | 界面节点与层级管理、MVVM 绑定、循环列表，以及适配、动画等扩展 |
| 本地化 | 语言选择、文本与字体处理、本地化文本组件和多语言 Key 检查工具 |
| 配置与存档 | Luban 生成的数据类与 JSON 配置加载、存档、用户设置 |
| 运行时服务 | 音频、特效、相机、图集、对象池、事件、引导和徽章；业务层另有红点与图标服务 |
| 网络与平台 | 网络消息封装、Protobuf 协议工具、Mirror / FishNet 接入、Steam 与房间管理 |
| 战斗 | 实体与系统、节点式逻辑配置、技能、Buff、子物体、命中与伤害处理，以及玩法扩展 |
| 编辑器工具 | 框架配置、资源打包、UI 工具、协议生成、引用池查看、音频库编辑、模型挂点生成 |

Mirror、FishNet 和 Steam 的接入代码位于项目业务层，具体服务与回调由 `GameMain` 配置。第三方库的存在不代表所有目标平台都已完成适配；接入自己的项目时需核对对应实现。

## 工程结构

```text
Assets/
├── Framework/             # 主工程层（Unity.Model）：启动器、配置、通用工具
├── Hotfix/                # 热更程序集（Unity.Hotfix）
│   ├── Framework/         # 通用模块、基础能力、UI、本地化、实体与节点逻辑
│   ├── GameLogic/         # 当前项目的初始化、流程、模块、界面与服务实现
│   │   └── CoreGame/      # 战斗基础、内核及当前玩法
│   ├── Ctrl/              # 控件与界面扩展
│   ├── Config/            # 热更配置代码
│   └── Utility/           # MVVM、异步等待、自动引用等工具
├── Editor/                # 编辑器工具（Unity.Editor）
├── Plugins/               # 插件代码（Unity.Plugin）
├── 3rdPlugins/            # 第三方插件
├── Content/               # 游戏内容资源
├── GameConfig/            # 版本与运行配置源文件
├── Resources/             # 启动资源与生成后的运行配置
└── Scenes/                # 启动场景
DataTables/                # 数据表、Luban 配置与导出脚本
Message/                   # Protobuf 协议源文件与中间输出
Tools/                     # Luban 等工具
Packages/                  # Unity 包依赖
ProjectSettings/           # Unity 工程设置
```

通用能力优先查看 `Assets/Hotfix/Framework`，当前项目如何组装这些能力则查看 [GameMain.cs](Assets/Hotfix/GameLogic/Package/GameMain/GameMain.cs)。业务层的实现和资源并非新项目必须照搬的框架组成。

## 启动与热更新

启动入口为 [Launch.unity](Assets/Scenes/Launch.unity)，当前 Build Settings 也以此场景为入口。

```text
Launcher
  → 初始化应用配置、语言与启动界面
  → 请求版本信息（启用更新时）
  → 初始化 YooAsset 资源包
  → 获取资源版本、更新清单与下载资源
  → 清理资源缓存
  → 加载 Unity.Hotfix
  → LccHotfix.Init.Start
  → GameMain：安装模块、初始化服务并进入业务流程
```

启动状态机见 [LauncherOperation.cs](Assets/Framework/Launch/LauncherOperation.cs)。资源包包括 `DefaultPackage` 与 `RawFilePackage`。

资源运行模式由 `isEnablePatcher` 和运行环境共同决定：

| 环境 | 关闭更新 | 开启更新 |
| --- | --- | --- |
| Unity 编辑器 | EditorSimulateMode | HostPlayMode |
| Player | OfflinePlayMode | HostPlayMode |

代码加载逻辑见 [FsmStartGame.cs](Assets/Framework/Launch/FsmNode/FsmStartGame.cs)：在定义 `HybridCLR` 且非编辑器的条件下，加载 AOT 补充元数据及资源中的 `Unity.Hotfix.dll`；其他情况下使用当前已加载的热更程序集。编辑器模拟运行不等同于验证 Player 热更新。

## 快速开始

1. 使用 Unity **2023.2.20f1** 打开仓库根目录，等待包依赖恢复与脚本编译完成。
2. 检查 [GameConfig_Version.txt](Assets/GameConfig/GameConfig_Version.txt)。本地编辑器调试可保持 `isEnablePatcher` 为 `false`；启用更新时需按自己的环境配置版本服务与资源地址，文件中的本机路径不可直接用于其他机器。
3. 修改配置后，通过 **Lcc框架 → 工具箱 → 框架配置 → 加密GameConfig** 生成启动时读取的 `Resources/GameConfig_Build.txt` 和 `Resources/GameConfig_Version.txt`。
4. 打开 `Assets/Scenes/Launch.unity`，进入 Play Mode。启动会继续加载当前项目的配置、引导与界面等资源，需要保留相应资源和 YooAsset 收集配置。
5. 从 `Assets/Hotfix/GameLogic/Init/Init.cs` 和 `GameMain.cs` 阅读业务入口，再按需要修改流程、服务及玩法。

构建 Player 前，需按目标平台完成 HybridCLR 安装、代码与 AOT 元数据生成，并准备 YooAsset 资源包。项目的 **Lcc框架 → 工具箱 → 打包** 提供资源构建工具，应用构建使用 Unity 的 Build Settings，**HybridCLR → CopyAotDlls** 提供 AOT DLL 复制入口；具体参数应与目标平台和资源部署保持一致。

## 战斗框架

战斗代码位于 `Assets/Hotfix/GameLogic/CoreGame`，依赖方向为：

```text
BattleGameplay → BattleKernel → BattleFoundation
```

| 层 | 职责 |
| --- | --- |
| BattleFoundation | 定点数、空间结构、属性叠加、随机与逻辑配置基类等基础能力 |
| BattleKernel | 世界帧管线、通用组件与系统、技能 / Buff / 子物体节点，以及服务接口 |
| BattleGameplay | 当前玩法的配置与节点、服务实现、玩法系统和世界创建信息 |

内核定义接口、策略或委托，由玩法层注入差异。新玩法应在 Gameplay 层实现，避免让 Kernel 反向依赖项目业务。

帧管线为 `Initialize → Rule → Behavior → Move → Battle → SyncView → Late → Debug`，最后进行死亡处理。技能通过逻辑配置与节点组合，命中与伤害通过对应事件及系统处理。

分层职责、关键入口和扩展方式见 [战斗框架说明](Assets/Hotfix/GameLogic/CoreGame/战斗框架AI必读.md)。

## 数据与开发工具

- **数据表**：`DataTables` 使用 Luban 导出 C# 数据类与 JSON。Windows 脚本为 [gen.bat](DataTables/gen.bat)，依赖 .NET 8；执行前应在 `DataTables` 目录核对相对路径与输出目录。当前脚本将代码输出至 `Assets/Hotfix/GameLogic/Package/LubanConfig/Gen`，数据输出至 `Assets/Res/LubanConfig`，需确认后者与实际 YooAsset 收集位置一致。运行时配置通过 `luban` 标签加载。
- **网络协议**：在 `Message/proto` 维护 `.proto` 文件，通过 **Lcc框架 → 工具箱 → PB生成工具** 生成并导入业务消息代码。
- **运行调试**：提供重启游戏（F5）、退出账号（F6）和引用池查看器等工具。
- **内容辅助**：提供本地化文本转换与 Key 检查、音频库编辑、模型挂点生成等编辑器入口。

## 第三方依赖与许可

主要依赖包括 [HybridCLR](https://github.com/focus-creative-games/hybridclr)、[YooAsset](https://github.com/tuyoogame/YooAsset)、[Luban](https://github.com/focus-creative-games/luban)、Mirror、FishNet、Steamworks.NET，以及 DOTween、LitMotion、A* Pathfinding、Odin Inspector、Easy Save 等插件。包依赖见 `Packages/manifest.json`，随工程放置的插件见 `Assets/3rdPlugins`。

Lcc 框架采用 [MIT License](LICENSE)。第三方插件遵循各自的许可；Odin Inspector 等商业插件需按其授权条款使用。

## 交流与作者

- 项目地址：[404Lcc/Lcc](https://github.com/404Lcc/Lcc)
- Lcc 框架交流群：[679054092](https://jq.qq.com/?_wv=1027&k=KPEIZeok)
- 作者：[404Lccxy](https://github.com/404Lccxy)
- [个人项目](https://www.taptap.com/developer/6782)
