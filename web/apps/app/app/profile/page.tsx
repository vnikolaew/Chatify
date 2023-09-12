"use client";
import React, { useEffect, useMemo, useState } from "react";
import { useCurrentUserId, useFileUpload, useUserEmail } from "@hooks";
import {
   ChangeUserStatusModel,
   useChangeUserStatusMutation,
   useEditUserDetailsMutation,
   useGetUserDetailsQuery,
} from "@web/api";
import {
   Avatar,
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Input,
   Link,
   Skeleton,
   Spinner,
} from "@nextui-org/react";
import { Mail, Phone } from "lucide-react";
import { UserStatus } from "@openapi";
import { USER_STATUSES } from "@components/navbar";

export interface PageProps {}

const Page = ({}: PageProps) => {
   const meId = useCurrentUserId();
   const { data: meDetails } = useGetUserDetailsQuery(meId);
   const { userEmail, setUserEmail, validationState, errorMessage } =
      useUserEmail();
   const [phoneNumber, setPhoneNumber] = useState(``);
   const {
      fileUploadRef,
      handleFileUpload,
      clearFiles,
      attachedFilesUrls,
      attachedFiles,
   } = useFileUpload();

   const {
      data: _,
      error: changeStatusError,
      isLoading: changeStatusLoading,
      mutateAsync: changeUserStatus,
   } = useChangeUserStatusMutation();
   const {
      mutateAsync: editDetails,
      error: editError,
      isLoading: editLoading,
   } = useEditUserDetailsMutation();

   const handleChangeUserStatus = async (newStatus: string) => {
      await changeUserStatus({
         newStatus,
      } as ChangeUserStatusModel);
      console.log(newStatus);
   };

   const statusColor = useMemo<string>(() => {
      if (!meDetails) return "default";
      switch (meDetails.user.status) {
         case UserStatus.AWAY:
            return "bg-warning";
         case UserStatus.ONLINE:
            return "bg-success";
         case UserStatus.OFFLINE:
            return "bg-default";
      }
      return "default";
   }, [meDetails]);

   useEffect(() => {
      meDetails?.user?.email?.value && setUserEmail(meDetails.user.email.value);
   }, [meDetails]);

   useEffect(() => {
      meDetails?.user?.phoneNumbers?.length &&
         setPhoneNumber(meDetails.user.phoneNumbers[0].value);
   }, [meDetails]);

   async function handleSaveChanges() {
      await editDetails({
         phoneNumbers: [phoneNumber],
         profilePicture: attachedFiles?.[0]?.file,
      });
   }

   return (
      <section
         className={`w-full mt-2 min-h-[70vh] flex flex-col items-center`}
      >
         <div className={`flex w-1/4 flex-col gap-4 mt-4`}>
            <div className={`flex items-center gap-6`}>
               <div className={`flex flex-col items-center gap-4`}>
                  <Avatar
                     fallback={
                        <Skeleton className={`rounded-medium w-20 h-20`} />
                     }
                     color={"default"}
                     size={`lg`}
                     className={`w-30 h-30`}
                     radius={`sm`}
                     src={
                        attachedFilesUrls.get(attachedFiles?.[0]?.id) ??
                        meDetails?.user?.profilePicture.mediaUrl
                     }
                  />
                  <Button
                     onPress={(_) => fileUploadRef?.current.click()}
                     className={`text-xs w-full h-7`}
                     color={`default`}
                     variant={`bordered`}
                     size={`sm`}
                  >
                     Upload photo
                  </Button>
                  {!!attachedFiles.length && (
                     <div
                        className={`flex self-center items-center justify-center`}
                     >
                        <Link
                           onPress={(_) => clearFiles()}
                           className={`text-xs self-center cursor-pointer text-center mx-auto w-full`}
                           color={`primary`}
                           underline={`always`}
                           size={`sm`}
                        >
                           Remove photo
                        </Link>
                     </div>
                  )}
                  <input
                     onChange={handleFileUpload}
                     type={`file`}
                     hidden
                     ref={fileUploadRef}
                  />
               </div>

               <div
                  className={`flex flex-col justify-center items-start gap-0 h-full`}
               >
                  <span className={`text-foreground text-xl`}>
                     {meDetails?.user.displayName}
                  </span>
                  <span className={`text-default-400 text-small`}>
                     {meDetails?.user.userHandle &&
                        `@${meDetails?.user.userHandle}`}
                  </span>
                  <Dropdown size={`sm`}>
                     <DropdownTrigger>
                        <Button
                           variant={`bordered`}
                           radius={`sm`}
                           className={`gap-1 mt-1 py-0 h-7 text-xs px-2`}
                           startContent={
                              <div
                                 className={`w-2 h-2 rounded-full ${statusColor}`}
                              />
                           }
                           // size={`sm`}
                           color={`default`}
                        >
                           Change status
                        </Button>
                     </DropdownTrigger>
                     <DropdownMenu onAction={handleChangeUserStatus}>
                        {[...USER_STATUSES]
                           .filter((s) => s.status !== meDetails?.user?.status)
                           .map(({ status, color }, i) => (
                              <DropdownItem
                                 // color={color}
                                 variant={"shadow"}
                                 selectedIcon={null!}
                                 // className={`w-[140px]`}
                                 classNames={{
                                    wrapper: "w-[200px] text-small",
                                    title: `text-small`,
                                    base: `text-small`,
                                 }}
                                 startContent={
                                    <Avatar
                                       className={`w-2 mr-1 h-2`}
                                       icon={""}
                                       size={"sm"}
                                       color={color}
                                    />
                                 }
                                 key={status}
                              >
                                 {status}
                              </DropdownItem>
                           ))}
                     </DropdownMenu>
                  </Dropdown>
               </div>
            </div>
            <h2 className={`self-center text-xl mt-8`}>Edit your Profile</h2>
            <div className={`flex flex-col mt-0 items-start gap-12`}>
               <Input
                  value={userEmail}
                  onValueChange={setUserEmail}
                  errorMessage={errorMessage}
                  // @ts-ignore
                  validationState={validationState}
                  labelPlacement={`outside`}
                  isClearable
                  placeholder={`Enter your new e-mail`}
                  startContent={<Mail size={16} />}
                  size={`md`}
                  // className={`pl-8 gap-2`}
                  classNames={{
                     input: `pl-2 font-light`,
                     label: `text-lg`,
                  }}
                  label={`Email`}
                  variant={`faded`}
                  color={`default`}
                  radius={`sm`}
                  type={`text`}
               />

               <Input
                  value={phoneNumber}
                  onValueChange={setPhoneNumber}
                  // @ts-ignore
                  labelPlacement={`outside`}
                  isClearable
                  placeholder={`Enter your new phone number`}
                  startContent={<Phone size={16} />}
                  size={`md`}
                  // className={`pl-8 gap-2`}
                  classNames={{
                     input: `pl-2 font-light`,
                     label: `text-lg`,
                  }}
                  label={`Phone number`}
                  variant={`faded`}
                  color={`default`}
                  radius={`sm`}
                  type={`tel`}
               />
            </div>
            <div className={`flex justify-end mt-8`}>
               <Button
                  isLoading={editLoading}
                  spinner={<Spinner color={`white`} size={`sm`} />}
                  onPress={handleSaveChanges}
                  size={`md`}
                  variant={`solid`}
                  color={`primary`}
               >
                  Save changes
               </Button>
            </div>
         </div>
      </section>
   );
};

export default Page;
