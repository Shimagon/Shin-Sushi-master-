# 📋 引き継ぎメモ (Next Actions)

お疲れ様でした！本日の作業は「認証ロジックの実装（コード側）」まで完了しています。

## 🛑 ここで止まっています
**Supabase（データベース）へのテーブル作成作業**

コード（プログラム）は書き換えましたが、データベースの中身が空っぽなため、サインアップ時にエラーになります。

## 🚀 次回やること（再開手順）

### 1. SupabaseでSQLを実行する
Supabaseのダッシュボード > SQL Editor に行き、以下のSQLを実行してください。

```sql
-- テーブルリセット & 作成
DROP TABLE IF EXISTS public.invitation_codes CASCADE;
DROP TABLE IF EXISTS public.users CASCADE;

-- Usersテーブル
CREATE TABLE public.users (
  id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  username TEXT UNIQUE NOT NULL,
  avatar_url TEXT,
  points INTEGER DEFAULT 0,
  invited_by UUID REFERENCES public.users(id),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Users can view all profiles" ON public.users FOR SELECT USING (true);
CREATE POLICY "Users can update own profile" ON public.users FOR UPDATE USING (auth.uid() = id);
CREATE POLICY "Users can insert own profile" ON public.users FOR INSERT WITH CHECK (auth.uid() = id);

-- Introduction Codesテーブル（回数制限付き）
CREATE TABLE public.invitation_codes (
  code TEXT PRIMARY KEY,
  created_by UUID REFERENCES public.users(id) ON DELETE CASCADE,
  max_uses INTEGER, -- NULL = 無制限
  uses INTEGER DEFAULT 0,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
ALTER TABLE public.invitation_codes ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Anyone can view invitation codes" ON public.invitation_codes FOR SELECT USING (true);
CREATE POLICY "Users can create invitation codes" ON public.invitation_codes FOR INSERT WITH CHECK (auth.uid() = created_by);
CREATE POLICY "System can update invitation codes" ON public.invitation_codes FOR UPDATE USING (true);

-- 初期コード発行
INSERT INTO public.invitation_codes (code, max_uses, uses) VALUES
  ('WELCOME2024', NULL, 0); -- 無制限コード
```

### 2. 動作確認
SQL実行後、ローカルサーバーが動いていれば（`npm run dev`）、以下でサインアップを試してください。

*   URL: http://localhost:3000/signup
*   招待コード: `WELCOME2024`

### 3. 次の開発タスク
*   **招待コード発行画面の実装**: ログインしたユーザーが、自分の権限（Admin or 一般）に応じてコードを発行できる画面を作る必要があります。

---
ゆっくり休んでください！💤
