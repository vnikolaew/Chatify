"use client";
import React, { useEffect, useState } from "react";
import {
   Avatar,
   Badge,
   Button,
   ButtonProps,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownSection,
   DropdownTrigger,
   Image,
   Link,
   Listbox,
   ListboxItem,
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
   Popover,
   PopoverContent,
   PopoverTrigger,
   Spinner,
   Switch,
} from "@nextui-org/react";
import { usePathname } from "next/navigation";
import NextLink from "next/link";
import {
   ChangeUserStatusModel,
   useChangeUserStatusMutation,
   useGetMyClaimsQuery,
   useGetUserDetailsQuery,
   useSignOutMutation,
} from "@web/api";
import { isValidURL } from "./Greeting";
import ExitIcon from "./icons/ExitIcon";
import ProfileIcon from "./icons/ProfileIcon";
import { useTheme } from "next-themes";
import { UserStatus } from "@openapi";
import RightArrow from "./icons/RightArrow";
import { useIsUserLoggedIn } from "../hooks/useIsUserLoggedIn";

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

const USER_STATUSES = new Set<{
   status: UserStatus;
   color: ButtonProps["color"];
}>([
   { status: UserStatus.AWAY, color: "warning" },
   {
      status: UserStatus.ONLINE,
      color: "success",
   },
   { status: UserStatus.OFFLINE, color: "default" },
]);

const MainNavbar = ({ baseImagesUrl }: { baseImagesUrl: string }) => {
   const pathname = usePathname();
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const [loadingAction, setLoadingAction] = useState<string>(null!);

   const {
      data: _,
      error: changeStatusError,
      isLoading: changeStatusLoading,
      mutateAsync: changeUserStatus,
   } = useChangeUserStatusMutation();
   const { data, isLoading, error } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });
   const { data: userDetails } = useGetUserDetailsQuery(
      data?.claims?.nameidentifier,
      {
         enabled: !!data?.claims?.nameidentifier,
      }
   );

   const { theme, setTheme } = useTheme();
   const [isStatusPopoverOpen, setIsStatusPopoverOpen] = useState(false);
   const [isDropdownMenuOpen, setIsDropdownMenuOpen] = useState(false);
   const { mutateAsync: signOut } = useSignOutMutation();

   const handleSignOut = async () => {
      await signOut(null!);
      window.location.reload();
   };

   useEffect(() => {
      if (!isDropdownMenuOpen) setIsStatusPopoverOpen(false);
   }, [isDropdownMenuOpen]);

   const handleChangeUserStatus = async (newStatus: string) => {
      setLoadingAction(newStatus);
      await changeUserStatus({
         newStatus,
      } as ChangeUserStatusModel);
      console.log(newStatus);
      setLoadingAction(null!);
      setIsStatusPopoverOpen(false);
   };

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
               <NavbarItem>
                  <Dropdown
                     showArrow
                     offset={10}
                     onOpenChange={setIsDropdownMenuOpen}
                     isOpen={changeStatusLoading || isDropdownMenuOpen}
                  >
                     <DropdownTrigger>
                        <Button
                           color={"default"}
                           variant={"light"}
                           size={"md"}
                           startContent={
                              <Badge // disableOutline
                                 content={""}
                                 shape={"circle"}
                                 color={
                                    userDetails?.user.status ===
                                    UserStatus.ONLINE
                                       ? "success"
                                       : userDetails?.user.status ===
                                         UserStatus.AWAY
                                       ? "warning"
                                       : "default"
                                 }
                                 size={"sm"}
                                 placement={"bottom-right"}
                              >
                                 <Avatar
                                    src={
                                       isValidURL(data?.claims.picture)
                                          ? data?.claims.picture
                                          : `${baseImagesUrl}/${data.claims.picture}`
                                    }
                                    size={"md"}
                                    alt={"profile-picture"}
                                    radius={"full"}
                                    color={"default"}
                                 />
                              </Badge>
                           }
                           className={`flex py-2 items-center gap-4`}
                        >
                           <span
                              className={`text-small light:text-red dark:text-foreground-500`}
                           >
                              {data.claims.name}
                           </span>
                        </Button>
                     </DropdownTrigger>
                     <DropdownMenu
                        className={`gap-2 min-w-[200px]`}
                        onAction={async (key) => {
                           if (key === "sign-out") {
                              await handleSignOut();
                           }
                           if (key === "status") {
                              console.log("im here");
                           }
                        }}
                        aria-label={"User actions"}
                     >
                        <DropdownSection showDivider>
                           <DropdownItem
                              textValue={"profile"}
                              classNames={{
                                 description: "text-[.75rem] text-default-300",
                              }}
                              description={"Manage your profile settings"}
                              className={`flex px-3 py-2 items-center gap-3`}
                              startContent={<ProfileIcon size={18} />}
                              key={"profile"}
                           >
                              <span className={`text-sm`}>Profile</span>
                           </DropdownItem>
                           <DropdownItem textValue={"rsdff"} key={"status-2"}>
                              <Popover
                                 color={"default"}
                                 size={"sm"}
                                 // keep popover open while request is in-flight:
                                 isOpen={
                                    changeStatusLoading || isStatusPopoverOpen
                                 }
                                 onOpenChange={setIsStatusPopoverOpen}
                                 offset={10}
                                 placement={"right"}
                              >
                                 <PopoverTrigger className={`px-1`}>
                                    <Button
                                       className={`bg-transparent text-small  w-full items-center justify-between`}
                                       size={"sm"}
                                       endContent={
                                          <RightArrow
                                             className={`fill:default-100 text-default-300`}
                                             size={24}
                                          />
                                       }
                                    >
                                       Change your status
                                    </Button>
                                 </PopoverTrigger>
                                 <PopoverContent>
                                    <Listbox
                                       onSelectionChange={console.log}
                                       onAction={handleChangeUserStatus}
                                       className={`w-[140px] px-0 py-1`}
                                       selectionMode={"single"}
                                       variant={"solid"}
                                       aria-label={"Statuses"}
                                    >
                                       {[...USER_STATUSES]
                                          .filter(
                                             (s) =>
                                                s.status !==
                                                userDetails?.user.status
                                          )
                                          .map(({ status, color }, i) => (
                                             <ListboxItem
                                                // color={color}
                                                variant={"shadow"}
                                                selectedIcon={null!}
                                                // className={`w-[140px]`}
                                                classNames={{
                                                   wrapper: "w-[300px]",
                                                }}
                                                endContent={
                                                   changeStatusLoading &&
                                                   status === loadingAction && (
                                                      <Spinner
                                                         color={color}
                                                         size={"sm"}
                                                      />
                                                   )
                                                }
                                                startContent={
                                                   <Avatar
                                                      className={`w-3 mr-1 h-3`}
                                                      icon={""}
                                                      size={"sm"}
                                                      color={color}
                                                   />
                                                }
                                                key={status}
                                             >
                                                {status}
                                             </ListboxItem>
                                          ))}
                                    </Listbox>
                                 </PopoverContent>
                              </Popover>
                           </DropdownItem>
                           <DropdownItem
                              textValue={"darkMode"}
                              endContent={
                                 <Switch
                                    size={"sm"}
                                    onChange={(e) => e.stopPropagation()}
                                    onClick={(e) => e.stopPropagation()}
                                    onValueChange={(_) => {
                                       setTheme(_ ? "dark" : "light");
                                    }}
                                    isSelected={theme === "dark"}
                                 />
                              }
                              className={`px-3 py-2`}
                              key={"darkMode"}
                           >
                              <span className={`text-small`}>Dark Mode</span>
                           </DropdownItem>
                        </DropdownSection>
                        <DropdownSection>
                           <DropdownItem
                              textValue={"sign-out"}
                              className={`flex px-3 py-2 transition-colors duration-100 items-center gap-3 fill-current data-[hover=true]:text-danger`}
                              startContent={
                                 <ExitIcon
                                    className={`fill-current`}
                                    size={22}
                                 />
                              }
                              color={"danger"}
                              variant={"bordered"}
                              key={"sign-out"}
                           >
                              <span className={`text-small`}>Sign Out</span>
                           </DropdownItem>
                        </DropdownSection>
                        <DropdownSection>
                           <DropdownItem
                              // onClick={(e) => {
                              //    e.stopPropagation();
                              //    setIsDropdownMenuOpen(true);
                              // }}
                              isDisabled
                              textValue={"about"}
                              className={`cursor-default`}
                           >
                              <span
                                 className={`text-xs leading-2 break-words mt-6 text-default-500`}
                              >
                                 Chatify, Inc. © {new Date().getFullYear()}. All{" "}
                                 <br />
                                 rights reserved.
                              </span>
                           </DropdownItem>
                        </DropdownSection>
                     </DropdownMenu>
                  </Dropdown>
               </NavbarItem>
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
