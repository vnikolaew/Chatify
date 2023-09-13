import { profileClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface NewPasswordInput {
   oldPassword: string;
   newPassword: string;
}

export interface EditUserDetailsModel {
   username?: string;
   email?: string;
   displayName?: string;
   profilePicture?: File;
   phoneNumbers?: string[];
   newPasswordInput?: NewPasswordInput;
}

const editUserDetails = async (model: EditUserDetailsModel) => {
   const { profilePicture, ...rest } = model;
   const formData = new FormData();

   Object.entries(rest).forEach(([key, value]) => formData.append(key, value));
   if (profilePicture) {
      formData.append(`profilePicture`, profilePicture, profilePicture?.name);
   }

   const { status, data } = await profileClient.patchForm(`details`, formData, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useEditUserDetailsMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, EditUserDetailsModel, any>(editUserDetails, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("User details edited successfully: " + data),
      onSettled: (res) => console.log(res),
   });
};
