"use client";
import React, { useMemo } from "react";
import {
   Button,
   Input,
   Spinner,
} from "@nextui-org/react";
import { UserIcon } from "lucide-react";
import { useUserHandle } from "@hooks";
import {
   useFindUserByHandleQuery
} from "@web/api";
import { useTranslations } from "next-intl";
import UserSearchResultSection from "./UserSearchResultSection";
import { UserNotFound } from "./UserNotFound";
import SuggestedFriendsSection from "./SuggestedFriendsSection";


export interface PageProps {
}

const FriendsPage = ({}: PageProps) => {
   const { validationState, userHandle, setUserHandle, errorMessage } =
      useUserHandle();
   const {
      isLoading,
      refetch: search,
      error,
      isFetching,
      data: user,
   } = useFindUserByHandleQuery(userHandle, { enabled: false });
   const noUserFound = useMemo(
      () => !isLoading && !isFetching && !user,
      [isLoading, isFetching, user],
   );

   const handleClick = async () => {
      await search();
   };

   const t = useTranslations(`Friends`);

   // @ts-ignore
   return (
      <section
         className={`w-full min-h-[70vh] mt-12 flex flex-col items-center`}
      >
         <div className="flex flex-col w-1/3 gap-2">
            <h1 className={`text-2xl text-foreground`}>{t(`Title`)}</h1>
            <h2 className={`text-default-400 font-normal text-small`}>
               {t(`Description`)}
            </h2>
            <div className={`w-full mt-2 flex flex-col items-center gap-1`}>
               <Input
                  value={userHandle}
                  onValueChange={setUserHandle}
                  placeholder={`User#0000`}
                  // @ts-ignore
                  validationState={validationState}
                  errorMessage={errorMessage}
                  startContent={
                     <UserIcon className={`fill-foreground`} size={16} />
                  }
                  variant={"flat"}
                  description={
                     t(`DescriptionTwo`)
                  }
                  classNames={{
                     input: `pl-3`,
                  }}
                  className={`w-full shadow-md`}
                  size={"md"}
                  color={"default"}
                  isClearable
                  type={"text"}
               />
               <Button
                  isLoading={isLoading && isFetching}
                  onPress={handleClick}
                  spinner={<Spinner color={`white`} size={`sm`} />}
                  isDisabled={!!errorMessage || userHandle.length === 0}
                  className={`self-end px-6 disabled:cursor-not-allowed ${
                     userHandle.length === 0 && `hover:cursor-not-allowed`
                  }`}
                  disabled={userHandle.length === 0}
                  variant={"shadow"}
                  color={"primary"}
                  size={"md"}
               >
                  {isLoading && isFetching ? t(`Searching`) : t(`Search`)}
               </Button>
            </div>
            {user && <UserSearchResultSection user={user} />}
            {noUserFound && (<UserNotFound />)}
            <SuggestedFriendsSection />
         </div>
      </section>
   );
};


export default FriendsPage;
