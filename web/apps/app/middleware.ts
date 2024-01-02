import createMiddleware from "next-intl/middleware";
import { NextMiddleware, NextResponse } from "next/server";

const intlMiddleware = createMiddleware({
   // A list of all locales that are supported
   locales: ["en", "de"],

   // If this locale is matched, pathnames work without a prefix (e.g. `/about`)
   defaultLocale: "en",
});

function stackMiddlewares(functions: MiddlewareFactory[] = [], index = 0): NextMiddleware {
   const current = functions[index];
   if (current) {
      const next = stackMiddlewares(functions, index + 1);
      return current(next);
   }
   return () => NextResponse.next();
}


export type MiddlewareFactory = (middleware: NextMiddleware) => NextMiddleware;

const withAuth: MiddlewareFactory = (next) => {
   return async (request, _next) => {
      const isUserLoggedIn = request.cookies.has(process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME);
      const pathname = request.nextUrl.pathname;

      if (isUserLoggedIn && (pathname === "/signin")) {
         request.nextUrl.pathname = `/`;
         return NextResponse.redirect(request.nextUrl);
      }

      if (!isUserLoggedIn && pathname !== "/signin") {
         request.nextUrl.pathname = `signin`;
         return NextResponse.redirect(request.nextUrl);
      }

      return intlMiddleware(request);
   };
};

export default stackMiddlewares([withAuth]);

export const config = {
   // Skip all paths that should not be internationalized. This example skips
   // certain folders and all pathnames with a dot (e.g. favicon.ico)
   matcher: ["/((?!api|_next|_vercel|.*\\..*).*)"],
};
