import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  expireTime: 0,
  experimental: {
    allowedDevOrigins: [
      "skid.gtag-api.win",
      "skid-api.gtag-api.win"
    ],
  },
};

export default nextConfig;
