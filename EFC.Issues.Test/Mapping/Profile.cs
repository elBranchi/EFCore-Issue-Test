using EFC.Issues.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFC.Issues.Test.Mapping
{
    public class Profile : AutoMapper.Profile
    {

        public Profile()
        {

            CreateMap<CLIENT_CONTACT, ClientContact>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.CONTACT_NAME))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.CONTACT_EMAIL))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.CONTACT_PHONE))
                ;

            CreateMap<ORDER_DETAIL, OrderDetail>()
                .ForMember(d => d.OrderId, opt => opt.MapFrom(s => s.ORDER_ID))
                .ForMember(d => d.ClientId, opt => opt.MapFrom(s => s.CLIENT_ID))
                .ForMember(d => d.BillingType, opt => opt.MapFrom(s => s.BILLING_TYPE))
                .ForMember(d => d.BillingContact, opt => opt.MapFrom(s => s.CLIENT_BILLING_CONTACT))
                .ForMember(d => d.ShippingContact, opt => opt.MapFrom(s => s.CLIENT_SHIPPING_CONTACT))
                ;
        }
    }
}
