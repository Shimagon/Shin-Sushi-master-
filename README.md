# Shin Sushi Master (VR)

Unity + SteamVR で動く、寿司を握る VR ゲーム。  
左手で握ったシャリと、右手で持ったネタを合わせると寿司が完成する。

> **学習・チーム制作プロジェクト**。Quest / Vive 系 SteamVR 対応 HMD で動作。  
> 卒制本命ではなく、VR インタラクション設計の素振り。

## 📌 何を作ったか (What)

- VR 空間の寿司カウンターに立ち、左右の手で食材を掴んで寿司を握る
- 客 NPC (CustomerSpawner で動的生成) が注文を出し、注文通りの寿司を提供すると評価が上がる
- SteamVR Interaction System の Player rig をベースに、独自に CustomerSpawner と「シャリ + ネタ → 寿司」のマッチ判定を実装

## 🛠 技術スタック

- **Unity 2022.3+**
- **SteamVR Plugin (Asset Store 版)** — Interaction System / Player Prefab を継承利用
- 3D アセット (別途 Asset Store からインポート):
  - GASTRO Sushi Food Pack FREE
  - Cartoon FX Remaster (CFXR Magic Poof エフェクト)

## 📂 構成

```
Assets/
├── Scenes/SampleScene.unity            # メインシーン
├── steamVRplugin setting/              # SteamVR Asset Store 版を配置
│   └── SteamVR/InteractionSystem/      # Player prefab を継承利用
└── <自作スクリプト・モデル>
Packages/manifest.json
ProjectSettings/
```

## 🚀 セットアップ

1. リポジトリを clone
2. Unity 2022.3+ で開く
3. Asset Store から以下を import (ライセンスの都合でリポジトリには含めていない):
   - SteamVR Plugin
   - GASTRO Sushi Food Pack FREE
   - Cartoon FX Remaster
4. `Assets/Scenes/SampleScene.unity` を開く

## 🩹 トラブルシューティング

SteamVR のアセンブリ重複 / Missing Prefab エラーが出た場合は、過去の移行作業メモを参照:

- [README_MIGRATION.md](README_MIGRATION.md) — クイック対処法
- [STEAMVR_MIGRATION_REPORT.md](STEAMVR_MIGRATION_REPORT.md) — 詳細レポート (原因 / 解決 / 検証)

## 📝 ライセンス

本人作成コードは MIT 相当 (学習目的)。  
外部 Asset Store アセットはそれぞれのライセンスに従い、各自 import すること。

---

**作者**: [shimada / Shimagon](https://github.com/Shimagon)
