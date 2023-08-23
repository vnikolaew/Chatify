"use client";
import React, { Fragment } from "react";
import {
   Image,
   Link,
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
} from "@nextui-org/react";
import { usePathname } from "next/navigation";
import NextLink from "next/link";
import { useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import UserDropdown from "@components/navbar/UserDropdown";
import NotificationsDropdown from "@components/navbar/NotificationsDropdown";

const NAV_LINKS = [
   {
      href: `/_playgrounds`,
      label: "Playgrounds",
      color: "danger",
   },
   {
      href: `/signup`,
      label: "Sign Up",
   },
   {
      href: `/signin`,
      label: "Sign In",
   },
];

const MainNavbar = ({ baseImagesUrl }: { baseImagesUrl: string }) => {
   const pathname = usePathname();
   const { isUserLoggedIn } = useIsUserLoggedIn();

   const { data, isLoading, error } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   return (
      <Navbar
         classNames={{
            item: ["data-[active=true]:text-primary"],
            base: "py-2",
         }}
         className={`w-full mx-0 !max-w-[3000px]`}
         isBordered
      >
         <NavbarBrand>
            <Link href={`/`} as={NextLink} className={`text-3xl`}>
               <Image
                  radius={"md"}
                  width={36}
                  height={36}
                  src={`favicon.ico`}
               />
               <h2 className={`ml-2 text-foreground`}>Chatify</h2>
            </Link>
         </NavbarBrand>
         <NavbarContent justify={"end"} className={`flex gap-4`}>
            {data?.claims && Object.keys(data?.claims).length ? (
               <Fragment>
                  <NavbarItem>
                     <UserDropdown baseImagesUrl={baseImagesUrl} />
                  </NavbarItem>
                  <NavbarItem>
                     <NotificationsDropdown />
                  </NavbarItem>
               </Fragment>
            ) : (
               NAV_LINKS.map((link, i) => (
                  <NavbarItem isActive={pathname === link.href} key={i}>
                     <Link
                        size={"lg"}
                        color={(link.color as any) ?? "foreground"}
                        className={`text-foreground group-[data-active=true]:text-primary`}
                        href={link.href}
                     >
                        {link.label}
                     </Link>
                  </NavbarItem>
               ))
            )}
         </NavbarContent>
      </Navbar>
   );
};

export default MainNavbar;
