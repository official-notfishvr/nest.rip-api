import { getLoginUrl } from '../lib/config';

export default function Login() {
  const loginUrl = getLoginUrl();

  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <div className="glass-panel p-12 rounded-3xl text-center max-w-md w-full">
        <h1 className="text-4xl font-bold mb-4 bg-gradient-to-r from-[#6c5ce7] to-[#00cec9] bg-clip-text text-transparent">
          NestRip Manager
        </h1>
        <p className="text-zinc-400 mb-8 text-lg">
          Manage your files and shorts with style.
        </p>

        <a
          href={loginUrl}
          className="inline-block px-8 py-3 rounded-xl bg-[#6c5ce7] hover:bg-[#a29bfe] text-white font-semibold transition-all hover:-translate-y-1 shadow-lg shadow-indigo-500/30"
        >
          Login with Nest.Rip
        </a>
      </div>

      <footer className="mt-8 text-zinc-600 text-sm">
        Powered by Nest.Rip API
      </footer>
    </div>
  );
}
