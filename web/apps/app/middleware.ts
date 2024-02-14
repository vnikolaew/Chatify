import createMiddleware from "next-intl/middleware";
import { NextMiddleware, NextResponse } from "next/server";

const LOCALES = ["en", "de"] as const;

const intlMiddleware = createMiddleware({
   // A list of all locales that are supported
   locales: LOCALES,

   // If this locale is matched, pathnames work without a prefix (e.g. `/about`)
   defaultLocale: "en",
});

function stackMiddlewares(
   functions: MiddlewareFactory[] = [],
   index = 0
): NextMiddleware {
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
      const isUserLoggedIn = request.cookies.has(
         process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
      );
      const signInRoutes = [`signin`, `/signin`] as const;

      let pathname = request.nextUrl.pathname;
      const match = pathname.match(/\/([^\/]+)\/([^\/]+)/);
      if (match?.length >= 2 && LOCALES.some((l) => match[1].includes(l))) {
         pathname = match[2];
      }

      if (isUserLoggedIn && signInRoutes.some((r) => pathname === r)) {
         request.nextUrl.pathname = `/`;
         return NextResponse.redirect(request.nextUrl);
      }

      if (!isUserLoggedIn && signInRoutes.every((r) => r !== pathname)) {
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
