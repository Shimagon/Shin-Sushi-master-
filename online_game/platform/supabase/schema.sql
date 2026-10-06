-- Users table (extends Supabase auth.users)
CREATE TABLE public.users (
  id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  username TEXT UNIQUE NOT NULL,
  avatar_url TEXT,
  points INTEGER DEFAULT 0,
  premium_currency INTEGER DEFAULT 0,
  invited_by UUID REFERENCES public.users(id),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable RLS
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;

-- Users policies
CREATE POLICY "Users can view all profiles" ON public.users
  FOR SELECT USING (true);

CREATE POLICY "Users can update own profile" ON public.users
  FOR UPDATE USING (auth.uid() = id);

CREATE POLICY "Users can insert own profile" ON public.users
  FOR INSERT WITH CHECK (auth.uid() = id);

-- Invitation codes table
CREATE TABLE public.invitation_codes (
  code TEXT PRIMARY KEY,
  created_by UUID REFERENCES public.users(id) ON DELETE CASCADE,
  max_uses INTEGER, -- NULL means unlimited
  uses INTEGER DEFAULT 0,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable RLS
ALTER TABLE public.invitation_codes ENABLE ROW LEVEL SECURITY;

-- Invitation codes policies
CREATE POLICY "Anyone can view invitation codes" ON public.invitation_codes
  FOR SELECT USING (true);

CREATE POLICY "Users can create invitation codes" ON public.invitation_codes
  FOR INSERT WITH CHECK (auth.uid() = created_by);

CREATE POLICY "System can update invitation codes" ON public.invitation_codes
  FOR UPDATE USING (true);

-- Create some initial invitation codes for testing
INSERT INTO public.invitation_codes (code, max_uses, uses) VALUES
  ('WELCOME2024', NULL, 0), -- Unlimited
  ('GAMER2024', 100, 0),
  ('INVITE001', 1, 0);

-- Function to generate random invite code
CREATE OR REPLACE FUNCTION generate_invite_code()
RETURNS TEXT AS $$
DECLARE
  characters TEXT := 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
  result TEXT := '';
  i INTEGER;
BEGIN
  FOR i IN 1..8 LOOP
    result := result || substr(characters, floor(random() * length(characters) + 1)::integer, 1);
  END LOOP;
  RETURN result;
END;
$$ LANGUAGE plpgsql;
