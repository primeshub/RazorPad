﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace RazorPad.Framework
{
    public class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        readonly IDictionary<string,object> _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] props =
                _dictionary.Keys
                    .Select(key => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key))
                    .Select(key => new DictionaryPropertyDescriptor(_dictionary, key))
                    .ToArray();

            return new PropertyDescriptorCollection(props);
        }
    }
}