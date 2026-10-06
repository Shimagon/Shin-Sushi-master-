# 🤖 Coding Agentへの指令書 (`prompt_for_coder.md`)

あなたは世界最高峰のフルスタックエンジニアです。
以下の `仕様書.md` に基づき、オンラインゲームプラットフォームの基盤と第1弾ゲーム「My Mysterious Creatures」を実装してください。

## 🛠 技術スタック (Confirmed)
*   **Framework**: Next.js 16+ (App Router)
*   **Language**: TypeScript
*   **Styling**: Tailwind CSS v4
*   **Real-time**: Socket.io (client & server) or Supabase Realtime
*   **Backend/DB**: Supabase (Auth & Database)
*   **UI Components**: shadcn/ui (Mobile First Customization)

## 📋 実装ミッション

### Step 1: プロジェクト & デザイン基盤 (Priority: High)
*   既存の `platform` ディレクトリを使用。
*   **Mobile First**: 全てのUIコンポーネントをスマホ操作（タッチターゲット、文字サイズ）に最適化。
*   **Theme**: "Friendly & Pop" (明るい色調、丸みのあるデザイン)。

### Step 2: 認証 & ロビー (Priority: High)
1.  **Supabase Auth**: 招待コード必須のサインアップフロー実装。
2.  **Lobby UI**: 
    *   QRコード表示によるルームシェア。
    *   入室済みのフレンド一覧表示。

### Step 3: ゲーム「My Mysterious Creatures」基盤
1.  **State Management**: クイズ/ゲーム進行のステート管理（Zustand等推奨）。
2.  **Game Logic**:
    *   カードめくり処理。
    *   **名前付け**: Web Speech API (音声) または Input 対応。
    *   **早押し**: Socket.io によるミリ秒単位の判定。
    *   **1台モード**: オフライン（ローカルステートのみ）での動作実装。

## 🚀 実行フロー
1.  `platform` フォルダの環境確認・整備。
2.  DBへのテーブル作成（Supabase SQL実行）。
3.  コンポーネント実装（アトミックに）。
4.  ページ結合。
4.  すべてのファイルを作成後、ビルドチェックを行う。

---
このファイルを読み込んだら、まずは「プロジェクトの初期化」から開始してください。
ユーザーへはこまめに進捗（コマンド実行の許可など）を確認してください。
