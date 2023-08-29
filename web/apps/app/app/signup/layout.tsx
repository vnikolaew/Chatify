import React, { PropsWithChildren } from "react";
import { OAuthProvider } from "../providers";

export interface LayoutProps extends PropsWithChildren {}

const Layout = ({ children }: LayoutProps) => {
   return (
      <section>
         <OAuthProvider>{children}</OAuthProvider>
      </section>
   );
};

export default Layout;
