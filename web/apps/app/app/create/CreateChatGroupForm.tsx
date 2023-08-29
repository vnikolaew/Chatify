"use client";
import React, { useMemo, useRef, useState } from "react";
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
} from "@nextui-org/react";
import UploadIcon from "@components/icons/UploadIcon";
import { useRouter } from "next/navigation";
import { User } from "@openapi";
import * as yup from "yup";

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
   const fileInputRef = useRef<HTMLInputElement>();
   const router = useRouter();
   const [selectedFile, setSelectedFile] = useState<File | null>();
   const fileUrl = useMemo(() => {
      if (!selectedFile) return "";
      return URL.createObjectURL(selectedFile);
   }, [selectedFile]);
   const normalizedFileName = useMemo(() => {
      if (!selectedFile) return "";
      const parts = selectedFile?.name.split(".");
      return `${parts
         .slice(0, parts.length - 1)
         .join("")
         .substring(0, 20)}.${parts.at(-1)}`;
   }, [selectedFile]);
   const {
      isLoading,
      error,
      mutateAsync: createChatGroup,
   } = useCreateChatGroupMutation();
   const {
      data: friends,
      isLoading: friendsLoading,
      error: friendsError,
   } = useGetMyFriendsQuery();

   const handleSubmit = async (model: CreateChatGroupModel) => {
      await createChatGroup(model, {
         onSuccess: (data) => {
            console.log(data);
            // router.push(`/?c=${(data as any)?.data?.groupId}&new=true`);
         },
      });
      console.log(model);
      console.log("form submitted");
   };

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
               className={`w-[320px] flex flex-col gap-3 mt-6`}
               autoComplete={"off"}
            >
               <Input
                  isClearable
                  onClear={() => setFieldValue("name", "")}
                  // value={values.name}
                  // onChange={handleChange}
                  {...getFieldProps("name")}
                  labelPlacement={"inside"}
                  isRequired
                  label={"Name"}
                  classNames={{
                     input: "text-small pb-1",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.name}
                  validationState={
                     touched.name && errors.name ? "invalid" : "valid"
                  }
                  placeholder={"Think of a cool name."}
                  size={"md"}
                  className={`py-1 text-md rounded-lg`}
                  type={"text"}
                  name={"name"}
                  id={"name"}
               />
               <Input
                  isClearable
                  onClear={() => setFieldValue("about", "")}
                  {...getFieldProps("about")}
                  labelPlacement={"inside"}
                  label={"About"}
                  classNames={{
                     input: "text-small pb-1",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.about}
                  validationState={
                     touched.about && errors.about ? "invalid" : "valid"
                  }
                  placeholder={"Give your group a brief description."}
                  size={"md"}
                  className={`py-1 text-md rounded-lg`}
                  type={"text"}
                  name={"about"}
                  id={"about"}
               />
               <div className={`flex items-center flex-col mt-2 gap-4`}>
                  <Input
                     className={`py-2 w-full`}
                     labelPlacement={"outside"}
                     label={"Select a group picture:"}
                     variant={"flat"}
                     color={"primary"}
                     classNames={{
                        inputWrapper: "py-2 px-4",
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
                     type={"text"}
                     isReadOnly
                     placeholder={
                        selectedFile ? normalizedFileName : " Upload a file"
                     }
                  />
                  {selectedFile && (
                     <div
                        className={`w-full flex gap-4 flex-col items-center justify-center`}
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
                        console.log(files[0]);
                        setSelectedFile(files[0]);
                        await setFieldValue("file", files[0]);
                        await setFieldTouched("file");
                     }}
                     name={"file"}
                     // value={selectedFile}
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
                     console.log([...ids]);
                     await setFieldValue("memberIds", [...ids]);
                     await setFieldTouched("memberIds");
                  }}
                  renderValue={(users: SelectedItems<User>) =>
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
                  description={"Add one or more of your friends to chat group"}
                  labelPlacement={"outside"}
                  placeholder={"Select users"}
                  items={friends ?? []}
               >
                  {(user) => (
                     <SelectItem key={user.id}>{user.username}</SelectItem>
                  )}
               </Select>
               <div
                  className={`flex-col self-center flex items-center gap-4 w-3/4`}
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
                     className={`text-white w-full mt-8 py-2 text-medium hover:opacity-80 self-center shadow-sm rounded-md`}
                     type={`submit`}
                  >
                     {isSubmitting ? "Loading" : "Create"}
                  </Button>
                  <Link
                     href={`/`}
                     className={`w-fit self-end`}
                     color={"foreground"}
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
