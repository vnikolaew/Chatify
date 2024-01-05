"use client";
import React from "react";
import { Formik } from "formik";
import {
   CreateChatGroupModel,
   getMediaUrl,
   useCreateChatGroupMutation,
   useGetMyFriendsQuery,
} from "@web/api";
import {
   Avatar,
   Button,
   Chip,
   Image,
   Input,
   Link,
   Select,
   SelectedItems,
   SelectItem,
   Spinner,
   User,
} from "@nextui-org/react";
import { useRouter } from "next/navigation";
import { User as TUser } from "@openapi";
import * as yup from "yup";
import { useQueryClient } from "@tanstack/react-query";
import { UploadIcon } from "@web/components";
import { useSingleFileUpload } from "@web/hooks";

export interface CreateChatGroupFormProps {}

const createChatGroupSchema = yup.object({
   about: yup.string().max(300, "About must be less than 300 characters."),
   // .required("About is required."),
   name: yup
      .string()
      .min(3, "Name must have more than 3 characters.")
      .max(100, "Name must have less than 100 characters.")
      .required("Name is required."),
   file: yup.mixed(),
   memberIds: yup
      .array()
      .of(yup.string())
      .nullable()
      .min(1, "Members must be more than 1.")
      .max(10, "Members must be less than 10."),
});

const CreateChatGroupForm = ({}: CreateChatGroupFormProps) => {
   const router = useRouter();
   const {
      selectedFile,
      fileUrl,
      setSelectedFile,
      fileInputRef,
      normalizedFileName,
   } = useSingleFileUpload();

   const {
      isPending,
      error,
      mutateAsync: createChatGroup,
   } = useCreateChatGroupMutation();

   const {
      data: friends,
      isLoading: friendsLoading,
      error: friendsError,
   } = useGetMyFriendsQuery();
   const client = useQueryClient();

   const handleSubmit = async (model: CreateChatGroupModel) =>
      await createChatGroup(model, {
         onSuccess: (data) => {
            client.refetchQueries({ exact: true, queryKey: [`feed`] });
            router.push(`/?c=${(data as any)?.data?.groupId}&new=true`);
         },
      });

   return (
      <Formik<CreateChatGroupModel>
         initialValues={{ about: "", file: null!, name: "", memberIds: [] }}
         validationSchema={createChatGroupSchema}
         validateOnMount={false}
         validateOnChange={true}
         onSubmit={handleSubmit}
      >
         {({
            values,
            errors,
            touched,
            setFieldValue,
            handleChange,
            handleSubmit,
            isSubmitting,
            getFieldProps,
            setFieldTouched,
         }) => (
            <form
               onSubmit={handleSubmit}
               className={`mt-8 flex w-[320px] flex-col gap-4`}
               autoComplete={"off"}
            >
               <Input
                  isClearable
                  onClear={() => setFieldValue("name", "")}
                  {...getFieldProps("name")}
                  labelPlacement={"outside"}
                  isRequired
                  label={"Name"}
                  classNames={{
                     input: "text-small pb-0",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.name}
                  validationState={
                     touched.name && errors.name ? "invalid" : "valid"
                  }
                  placeholder={"Think of a cool name."}
                  size={"md"}
                  radius={`sm`}
                  className={`text-md py-1`}
                  type={"text"}
                  name={"name"}
                  id={"name"}
               />
               <Input
                  isClearable
                  onClear={() => setFieldValue("about", "")}
                  {...getFieldProps("about")}
                  labelPlacement={"outside"}
                  label={"About"}
                  classNames={{
                     input: "text-small pb-0",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.about}
                  validationState={
                     touched.about && errors.about ? "invalid" : "valid"
                  }
                  placeholder={"Give your group a brief description."}
                  size={"md"}
                  radius={`sm`}
                  className={`text-md py-1`}
                  type={"text"}
                  name={"about"}
                  id={"about"}
               />
               <div className={`mt-2 flex flex-col items-center gap-4`}>
                  <Input
                     className={`w-full py-2`}
                     labelPlacement={"outside"}
                     label={"Select a group picture:"}
                     variant={"flat"}
                     color={"primary"}
                     onClick={(_) => fileInputRef.current?.click()}
                     classNames={{
                        inputWrapper: "py-2 px-2",
                        input: `cursor-pointer`,
                        label: `text-foreground text-medium`,
                     }}
                     startContent={
                        <Button
                           onPress={(_) => fileInputRef.current?.click()}
                           variant={"light"}
                           color={"primary"}
                           radius={"full"}
                           size={"sm"}
                           isIconOnly
                        >
                           <UploadIcon className={`fill-white`} size={20} />
                        </Button>
                     }
                     type={`text`}
                     isReadOnly
                     placeholder={
                        selectedFile ? normalizedFileName : " Upload a file"
                     }
                  />
                  {selectedFile && (
                     <div
                        className={`flex w-full flex-col items-center justify-center gap-4`}
                     >
                        <Image
                           shadow={"md"}
                           className={`mx-auto`}
                           radius={"md"}
                           src={fileUrl}
                           width={120}
                           height={120}
                        />
                     </div>
                  )}
                  <input
                     onChange={async ({ target: { files } }) => {
                        setSelectedFile(files[0]);
                        await setFieldValue("file", files[0]);
                        await setFieldTouched("file");
                     }}
                     name={"file"}
                     ref={fileInputRef}
                     hidden
                     type={"file"}
                  />
               </div>
               <Select
                  variant={"underlined"}
                  selectionMode={"multiple"}
                  color={"primary"}
                  onSelectionChange={async (ids) => {
                     await setFieldValue("memberIds", [...ids]);
                     await setFieldTouched("memberIds");
                  }}
                  renderValue={(users: SelectedItems<TUser>) =>
                     users.map((user) => (
                        <Chip
                           startContent={
                              <Avatar
                                 radius={"full"}
                                 classNames={{
                                    base: "w-4 h-4",
                                    img: "w-4 h-4",
                                 }}
                                 size={`sm`}
                                 src={getMediaUrl(
                                    user.data.profilePicture.mediaUrl
                                 )}
                              />
                           }
                           className={`mr-2`}
                           size={"sm"}
                           color={"default"}
                           key={user.key}
                        >
                           <span className={`ml-1`}>
                              {user.data.username.at(0).toUpperCase()}
                           </span>
                        </Chip>
                     ))
                  }
                  name={"memberIds"}
                  classNames={{
                     base: `my-4`,
                     description: "mt-1 text-foreground-500",
                     label: `text-medium`,
                  }}
                  label={"Add friends to group:"}
                  description={
                     <span>
                        <b className={`text-primary mr-1`}>*</b> Add one or more
                        of your friends to chat group
                     </span>
                  }
                  labelPlacement={"outside"}
                  placeholder={"Select users"}
                  items={friends ?? []}
               >
                  {(user) => (
                     <SelectItem key={user.id}>
                        <User
                           avatarProps={{
                              src: getMediaUrl(user.profilePicture.mediaUrl),
                              size: `sm`,
                              color: "danger",
                              className: `w-6 h-6`,
                              radius: `full`,
                           }}
                           classNames={{
                              name: `text-small`,
                           }}
                           name={user.username}
                        />
                     </SelectItem>
                  )}
               </Select>
               <div
                  className={`flex w-3/4 flex-col items-center gap-4 self-center`}
               >
                  <Button
                     variant={"solid"}
                     isLoading={isSubmitting}
                     spinner={
                        <Spinner
                           className={`mr-2`}
                           color={"white"}
                           size={"sm"}
                        />
                     }
                     color={"primary"}
                     size={"md"}
                     className={`text-medium mt-4 w-full self-center rounded-md py-2 text-white shadow-sm hover:opacity-80`}
                     type={`submit`}
                  >
                     {isSubmitting ? "Creating" : "Create"}
                  </Button>
                  <Link
                     href={`/`}
                     className={`w-fit self-end`}
                     color={"primary"}
                     underline={"hover"}
                     size={"sm"}
                  >
                     &#8592; Go back
                  </Link>
               </div>
            </form>
         )}
      </Formik>
   );
};
export default CreateChatGroupForm;
