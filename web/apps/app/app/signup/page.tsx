"use client";
import React from "react";
import { NextPage } from "next";
import { RegularSignUpModel, sleep, useRegularSignUpMutation } from "@web/api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
   Button,
   Card,
   CardBody,
   CardHeader,
   Input,
   Spinner,
} from "@nextui-org/react";
import * as yup from "yup";
import { Formik } from "formik";
import PasswordInput from "../signin/PasswordInput";

const EMAIL_REGEX = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
const PASSWORD_REGEX =
   /^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{3,}$/;

const signUpSchema = yup.object({
   email: yup
      .string()
      .max(100, "Email must be less than 100 characters.")
      .required("Email is required.")
      .matches(EMAIL_REGEX, { message: "Invalid email." }),
   username: yup
      .string()
      .min(3, "Username must have more than 3 characters.")
      .max(50, "Username must have less than 50 characters.")
      .required("Username is required."),
   password: yup
      .string()
      .min(3, "Password must have more than 3 characters.")
      .max(50, "Password must have less than 50 characters.")
      .required("Password is required.")
      .matches(PASSWORD_REGEX, { message: "Invalid password." }),
});

const SignUpPage: NextPage = () => {
   const router = useRouter();
   const { data, mutateAsync: signUp, error } = useRegularSignUpMutation();

   async function handleFormSubmit(data: RegularSignUpModel) {
      try {
         // await signIn(signInModel);
         await sleep(2000);
         // router.push(`/`, { forceOptimisticNavigation: false });
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`w-1/4 mx-auto mt-24`}>
         <Card shadow={"lg"} className={`p-6`} radius={"md"}>
            <CardHeader className={`text-2xl`}>Create an account</CardHeader>
            <CardBody className={`mt-4`}>
               <Formik<RegularSignUpModel>
                  validationSchema={signUpSchema}
                  initialValues={{ email: "", password: "", username: "" }}
                  onSubmit={handleFormSubmit}
               >
                  {({
                     values,
                     errors,
                     setFieldValue,
                     touched,
                     handleChange,
                     handleSubmit,
                     isSubmitting,
                  }) => (
                     <form
                        autoComplete={"off"}
                        noValidate
                        className={`flex flex-col gap-3`}
                        onSubmit={handleSubmit}
                     >
                        <Input
                           autoFocus
                           isClearable
                           onClear={() => setFieldValue("username", "")}
                           value={values.username}
                           onChange={handleChange}
                           label={"Username"}
                           autoComplete={"off"}
                           errorMessage={errors?.username}
                           validationState={
                              touched.username && errors.username
                                 ? "invalid"
                                 : "valid"
                           }
                           placeholder={"How are people going to call you?"}
                           size={"lg"}
                           className={`py-1 text-md rounded-lg`}
                           type={"text"}
                           name={"username"}
                           id={"username"}
                        />
                        <Input
                           autoFocus
                           isClearable
                           onClear={() => setFieldValue("username", "")}
                           value={values.email}
                           onChange={handleChange}
                           label={"Email"}
                           autoComplete={"off"}
                           errorMessage={errors?.email}
                           validationState={
                              touched.email && errors.email
                                 ? "invalid"
                                 : "valid"
                           }
                           placeholder={"Type in your email"}
                           size={"lg"}
                           className={`py-1 text-md rounded-lg`}
                           type={"email"}
                           name={"email"}
                           id={"email"}
                        />
                        <PasswordInput
                           size={"lg"}
                           value={values.password}
                           onChange={handleChange}
                           errorMessage={errors.password}
                           validationState={
                              touched.password && errors.password
                                 ? "invalid"
                                 : "valid"
                           }
                           label={"Password"}
                           placeholder={"Choose a strong one"}
                           className={`py-1 text-md rounded-lg`}
                           name={"password"}
                           id={"password"}
                        />
                        <div className={`w-full flex flex-col justify-between`}>
                           <Link
                              className={`hover:underline text-md text-blue-500`}
                              href={`/signin`}
                           >
                              Already have an account? Sign in here.
                           </Link>
                           <Button
                              variant={"solid"}
                              isLoading={isSubmitting}
                              spinner={
                                 <Spinner
                                    className={`mr-2`}
                                    color={"default"}
                                    size={"md"}
                                 />
                              }
                              color={"primary"}
                              size={"lg"}
                              className={`text-white mt-12 py-3 w-1/2 text-[1.2rem] hover:opacity-80 self-end shadow-sm rounded-md`}
                              type={`submit`}
                           >
                              {isSubmitting ? "Loading" : "Sign Up"}
                           </Button>
                        </div>
                     </form>
                  )}
               </Formik>
            </CardBody>
            {/*<CardFooter></CardFooter>*/}
         </Card>
      </div>
   );
};

export default SignUpPage;
