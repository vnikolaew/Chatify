//@ts-check

// eslint-disable-next-line @typescript-eslint/no-var-requires
const { composePlugins, withNx } = require("@nx/next");

/**
 * @type {import('@nrwl/next/plugins/with-nx').WithNxOptions}
 **/
const nextConfig = {
   nx: {
      // Set this to true if you would like to to use SVGR
      // See: https://github.com/gregberge/svgr
      svgr: false,
   },
   typescript: {
      ignoreBuildErrors: true,
   },
   images: {
      formats: ["image/webp"],
      loader: "default",
      unoptimized: true,
      disableStaticImages: false,
      minimumCacheTTL: 60 * 60,
      domains: [],
   },
   async headers() {
      return Promise.resolve([
         {
            source: "/:path*",
            headers: [{ key: "X-Server", value: "Chatify/Next" }],
         },
      ]);
   },
   experimental: { appDir: true },
   reactStrictMode: false,
   optimizeFonts: true,
   poweredByHeader: true,
};

const plugins = [
   // Add more Next.js plugins to this list if needed.
   withNx,
];

module.exports = composePlugins(...plugins)(nextConfig);
