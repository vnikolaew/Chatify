"use client";
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
   Link,
   Listbox,
   ListboxItem,
   Popover,
   PopoverContent,
   PopoverTrigger,
   Spinner,
   Switch,
} from "@nextui-org/react";
import React, { useEffect, useMemo, useState } from "react";
import {
   ChangeUserStatusModel,
   getMediaUrl,
   useChangeUserStatusMutation,
   useGetMyClaimsQuery,
   useGetUserDetailsQuery,
   useSignOutMutation,
} from "@web/api";
import { useTheme } from "next-themes";
import { useIsUserLoggedIn } from "@hooks";
import { UserStatus } from "@openapi";
import { ExitIcon, PlusIcon, ProfileIcon, RightArrow } from "@icons";
import ChatBubbleIcon from "@components/icons/ChatBubbleIcon";
import { useTranslations } from "next-intl";

export const USER_STATUSES = new Set<{
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

export interface UserDropdownProps {
}

export const UserDropdown = ({}: UserDropdownProps) => {
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const { data, isLoading, error } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   const { data: userDetails } = useGetUserDetailsQuery(
      data?.claims?.nameidentifier,
      {
         enabled: !!data?.claims?.nameidentifier,
      },
   );

   const {
      data: _,
      error: changeStatusError,
      isLoading: changeStatusLoading,
      mutateAsync: changeUserStatus,
   } = useChangeUserStatusMutation();

   const { theme, setTheme } = useTheme();
   const [loadingAction, setLoadingAction] = useState<string>(null!);
   const [isStatusPopoverOpen, setIsStatusPopoverOpen] = useState(false);
   const [isDropdownMenuOpen, setIsDropdownMenuOpen] = useState(false);
   const visibleStatuses = useMemo(() => {
      return [...USER_STATUSES]
         .filter(
            (s) => s.status !== userDetails?.user.status,
         );
   }, [userDetails]);

   const t = useTranslations(`MainNavbar`);

   useEffect(() => {
      if (!isDropdownMenuOpen) setIsStatusPopoverOpen(false);
   }, [isDropdownMenuOpen]);

   const handleChangeUserStatus = async (newStatus: string) => {
      setLoadingAction(newStatus);

      await changeUserStatus({ newStatus } as ChangeUserStatusModel);

      setLoadingAction(null!);
      setIsStatusPopoverOpen(false);
   };
   const { mutateAsync: signOut } = useSignOutMutation();

   const handleSignOut = async () => {
      signOut(null!).then(_ => window.location.reload());
   };

   return (
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
                  <Badge
                     content={""}
                     shape={"circle"}
                     color={
                        userDetails?.user.status === UserStatus.ONLINE
                           ? "success"
                           : userDetails?.user.status === UserStatus.AWAY
                              ? "warning"
                              : "default"
                     }
                     classNames={{
                        badge: `rounded-full h-3 w-3`,
                        // base: `w-2 h-2`,
                     }}
                     size={"sm"}
                     variant={`shadow`}
                     placement={"bottom-right"}
                  >
                     <Avatar
                        src={getMediaUrl(data?.claims?.picture)}
                        size={"sm"}
                        alt={"profile-picture"}
                        radius={"full"}
                        color={"default"}
                     />
                  </Badge>
               }
               className={`flex py-2 items-center gap-2`}
            >
               <span
                  className={`text-xs light:text-red dark:text-foreground-600`}
               >
                  {data.claims.name}
               </span>
            </Button>
         </DropdownTrigger>
         <DropdownMenu
            className={`gap-2 min-w-[200px]`}
            onAction={async (key) => {
               switch (key) {
                  case "sign-out":
                     await handleSignOut();
                     break;
                  case "status":
                     setIsDropdownMenuOpen(true);
                     break;
                  case "create-chat-group":
                     setIsDropdownMenuOpen(false);
                     break;
                  default:
                     break;
               }
            }}
            aria-label={"User actions"}
         >
            <DropdownSection showDivider>
               <DropdownItem
                  textValue={"profile"}
                  as={Link}
                  // @ts-ignore
                  href={`/profile`}
                  classNames={{
                     description: "text-[.6rem] text-default-300 h-6",
                     title: `h-fit text-xs`,
                  }}
                  description={t(`Profile.description`)}
                  className={` flex px-3 py-2 text-foreground items-center gap-3`}
                  startContent={<ProfileIcon size={18} />}
                  key={"profile"}
               >
                  <span className={`text-xs h-auto`}>{t(`Profile.title`)}</span>
               </DropdownItem>
               <DropdownItem
                  as={Link}
                  href={`/create`}
                  // @ts-ignore
                  // color={"foreground"}
                  textValue={"create-chat-group"}
                  endContent={
                     <PlusIcon className={`fill-foreground ml-3`} size={20} />
                  }
                  classNames={{
                     description: "text-[.6rem] text-default-300 h-6",
                     title: `h-fit text-xs`,
                  }}
                  description={t(`CreateChatGroup.description`)}
                  startContent={
                     <ChatBubbleIcon className={`fill-foreground`} size={20} />
                  }
                  className={`px-3 py-2 text-foreground `}
                  key={"create-chat-group"}
               >
                  <span className={`text-xs`}>{t(`CreateChatGroup.title`)}</span>
               </DropdownItem>
               <DropdownItem textValue={"rsdff"} key={"status"}>
                  <Popover
                     color={"default"}
                     size={"sm"}
                     // keep popover open while request is in-flight:
                     isOpen={changeStatusLoading || isStatusPopoverOpen}
                     onOpenChange={open => {
                        if (isStatusPopoverOpen) setIsStatusPopoverOpen(false);
                        else {
                           setIsStatusPopoverOpen(open);
                        }
                     }}
                     offset={10}
                     placement={"right"}
                  >
                     <PopoverTrigger className={`px-1`}>
                        <Button
                           className={`bg-transparent text-xs w-full items-center justify-between`}
                           onClick={e => {
                              e.preventDefault();
                              e.stopPropagation();
                              setIsDropdownMenuOpen(true);
                           }}
                           size={"sm"}
                           endContent={
                              <RightArrow
                                 fill={`white`}
                                 size={22}
                              />
                           }
                        >
                           {t(`ChangeStatus.title`)}
                        </Button>
                     </PopoverTrigger>
                     <PopoverContent>
                        <Listbox
                           onSelectionChange={console.log}
                           className={`w-[140px] px-0 py-1`}
                           selectionMode={"single"}
                           variant={"solid"}
                           aria-label={"Statuses"}
                        >
                           {visibleStatuses
                              .map(({ status, color }, i) => (
                                 <ListboxItem
                                    // color={color}
                                    variant={"shadow"}
                                    selectedIcon={null!}
                                    onClick={_ => handleChangeUserStatus(status)}
                                    // className={`w-[140px]`}
                                    classNames={{
                                       wrapper: "w-[300px]",
                                    }}
                                    endContent={
                                       changeStatusLoading &&
                                       status === loadingAction && (
                                          <Spinner color={color} size={"sm"} />
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
                  <span className={`text-xs`}>{t(`DarkMode.title`)}</span>
               </DropdownItem>
            </DropdownSection>
            <DropdownSection>
               <DropdownItem
                  textValue={"sign-out"}
                  className={`flex px-3 py-2 transition-colors duration-100 items-center gap-3 fill-current data-[hover=true]:text-danger`}
                  startContent={
                     <ExitIcon className={`fill-current`} size={22} />
                  }
                  color={"danger"}
                  variant={"bordered"}
                  key={"sign-out"}
               >
                  <span className={`text-xs`}>{t(`SignOut.title`)}</span>
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
                     className={`text-[.6rem] leading-2 break-words mt-2 text-default-500`}
                  >
                     Chatify, Inc. © {new Date().getFullYear()}. All rights reserved.
                  </span>
               </DropdownItem>
            </DropdownSection>
         </DropdownMenu>
      </Dropdown>
   );
};
